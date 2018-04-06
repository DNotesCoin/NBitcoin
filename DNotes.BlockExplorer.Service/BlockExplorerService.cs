using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DNotes.Infrastructure;
using NBitcoin;

namespace DNotes.BlockExplorer.Service
{
	public class BlockExplorerService
	{

		public static Dictionary<uint256, Block> GetAllBlocks()
		{
			var blocks = new Dictionary<uint256, Block>();

			using (IDbConnection db = new SqlConnection(AppSettings.BlockExplorerConnectionString))
			{
				var query = @"Select * from Block";
				var dbBlocks = db.Query<dynamic>(query, new { }).Select(dynamicItem =>
				{
					var item = (IDictionary<string, object>) dynamicItem;
					var block = new Block();
					block.Header.hashHACK = new uint256(item["Hash"]?.ToString());
					block.Header.HashPrevBlock = new uint256(item["HashPrevBlock"]?.ToString());
					block.Header.HashMerkleRoot = new uint256(item["HashMerkleRoot"]?.ToString());

					//need to fully build block

					return block;
				}).ToList();

				foreach (var dbBlock in dbBlocks)
				{
					blocks.Add(dbBlock.Header.hashHACK, dbBlock);
				}
			}


			return blocks;
		}

		public static void AddBlock(Block block, Network network)
		{

			using (IDbConnection db = new SqlConnection(AppSettings.BlockExplorerConnectionString))
			{
				string sql = @"
				INSERT INTO 
				BLOCK ([Hash], [HashMerkleRoot], [HashPrevBlock], [Height], [Time], [Bits], [Version], [Nonce], [Type])
				VALUES (@hash, @hashMerkleRoot, @hashPrevBlock, @height, @time, @bits, @version, @nonce, @type)

				SELECT CAST(SCOPE_IDENTITY() as int)
				";

				var blockId = db.Query<int>(sql, 
					new
					{
						hash = block.Header.GetHash().ToString(), hashMerkleRoot = block.Header.HashMerkleRoot.ToString(), hashPrevBlock = block.Header.HashPrevBlock.ToString(),
						height = 1, time = block.Header.BlockTime, bits = 1, version = block.Header.Version, nonce = 1, type = 1 //type is proof of stake vs proof of work
					}).Single();

				foreach (var transaction in block.Transactions)
				{
					sql = @"
					INSERT INTO 
					[Transaction] ([BlockId], [Hash])
					VALUES (@blockId, @hash)

					SELECT CAST(SCOPE_IDENTITY() as int)
					";

					var transactionId = db.Query<int>(sql, new { blockId = blockId, hash = transaction.GetHash().ToString()}).Single();

					foreach (var input in transaction.Inputs)
					{
						sql = @"
						INSERT INTO 
						[TransactionInput] ([TransactionId], [Index])
						VALUES (@transactionId, @index)

						SELECT CAST(SCOPE_IDENTITY() as int)
						";

						var transactionInputId = db.Query<int>(sql, new { transactionId = transactionId, index = (int) input.PrevOut.N + 1 }).Single();

					}

					foreach (var output in transaction.Outputs)
					{
						if (output.ScriptPubKey.Length != 0)
						{
							string address = "";
							ScriptTemplate scriptTemplate = output.ScriptPubKey.FindTemplate();
							switch (scriptTemplate.Type)
							{
								// Pay to PubKey can be found in outputs of staking transactions.
								case TxOutType.TX_PUBKEY:
									PubKey pubKey = PayToPubkeyTemplate.Instance.ExtractScriptPubKeyParameters(output.ScriptPubKey);
									address = pubKey.GetAddress(network).ToString();
									break;
								// Pay to PubKey hash is the regular, most common type of output.
								case TxOutType.TX_PUBKEYHASH:
									address = output.ScriptPubKey.GetDestinationAddress(network).ToString();
									break;
								case TxOutType.TX_NONSTANDARD:
								case TxOutType.TX_SCRIPTHASH:
								case TxOutType.TX_MULTISIG:
								case TxOutType.TX_NULL_DATA:
								case TxOutType.TX_SEGWIT:
									break;
							}

							sql = @"

							IF EXISTS(SELECT *  FROM  [Address] WHERE Address=@address)
							BEGIN
								SELECT top 1 Id FROM  [Address] WHERE Address=@address
							END
							ELSE
							BEGIN
								INSERT INTO 
								[Address] ([Address])
								VALUES (@address)
							
								SELECT  CAST(SCOPE_IDENTITY() as int)
							END
							";

							var addressId = db.Query<int>(sql, new {address = address}).Single();

							sql = @"
							INSERT INTO 
							[TransactionOutput] ([TransactionId], [AddressId], [Value])
							VALUES (@transactionId, @addressId, @value)

							SELECT CAST(SCOPE_IDENTITY() as int)
							";

							var transactionOutputId =
								db.Query<int>(sql, new {transactionId = transactionId, addressId = addressId, value = output.Value.ToDecimal(MoneyUnit.Satoshi)}).Single();

						}
					}

				}


			}

		}

		public static List<Transaction> GetTransactionsForAddress(string address)
		{
			using (IDbConnection db = new SqlConnection(AppSettings.BlockExplorerConnectionString))
			{
				var sql = @"
				select	t.*
				from	Address a inner join 
						TransactionOutput txo on a.id = txo.addressid inner join 
						[Transaction] t on txo.TransactionId = t.id
				where	a.Address = @address
				";
				var dbTransactions = db.Query<dynamic>(sql, new { address}).Select(dynamicItem =>
				{
					var item = (IDictionary<string, object>)dynamicItem;
					var transaction = new Transaction();
					transaction.hashHACK = new uint256(item["Hash"]?.ToString());

					return transaction;
				}).ToList();

				return dbTransactions;
			}
		}
	}
}
