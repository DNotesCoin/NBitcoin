using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NBitcoin;

namespace DNotes.BlockExplorer.Service
{
	public class BlockExplorerService
	{

		public const string ConnectionString =
			"Data Source=publicrepositoriesarefunt;Persist Security Info=True;User ID=public;Password=publicrepositoriesarefun!;initial catalog=DNotes.BlockExplorer;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";

		public static Dictionary<uint256, Block> GetAllBlocks()
		{
			var blocks = new Dictionary<uint256, Block>();

			using (IDbConnection db = new SqlConnection(ConnectionString))
			{
				var query = @"Select * from Block";
				var dbBlocks = db.Query<dynamic>(query, new { }).Select(dynamicItem =>
				{
					var item = (IDictionary<string, object>) dynamicItem;
					var block = new Block();
					block.Header.HashPrevBlock = new uint256(item["HashPrevBlock"]?.ToString());
					block.Header.HashMerkleRoot = new uint256(item["HashMerkleRoot"]?.ToString());
					//block.Header.BlockTime = new uint256(item["HashMerkleRoot"]?.ToString());

					return block;
				}).ToList();

			}

			return blocks;
		}

		public static void AddBlock(Block block)
		{

			using (IDbConnection db = new SqlConnection(ConnectionString))
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
						hash = block.Header.GetHash().ToString(), hashMerkleRoot = block.Header.HashPrevBlock.ToString(), hashPrevBlock = block.Header.HashPrevBlock.ToString(),
						height = 1, time = block.Header.BlockTime, bits = 1, version = block.Header.Version, nonce = 1, type = 1
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

						var transactionInputId = db.Query<int>(sql, new { transactionId = transactionId, index = (int) input.PrevOut.N }).Single();

					}

					foreach (var output in transaction.Outputs)
					{
						var address = output.ScriptPubKey.GetScriptAddress(Network.RegTest).ToString().Substring(0, 34).ToString();
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

						var addressId = db.Query<int>(sql, new { address = address }).Single();

						sql = @"
						INSERT INTO 
						[TransactionOutput] ([TransactionId], [AddressId], [Value])
						VALUES (@transactionId, @addressId, @value)

						SELECT CAST(SCOPE_IDENTITY() as int)
						";

						var transactionOutputId = db.Query<int>(sql, new { transactionId = transactionId, addressId = addressId, value = output.Value.ToDecimal(MoneyUnit.Satoshi) }).Single();

					}

				}


			}

		}
	}
}
