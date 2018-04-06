using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using DNotes.BlockExplorer.Service;
using DNotes.Infrastructure;
using NBitcoin;
using NBitcoin.BitcoinCore;

namespace DNotes.BlockExplorer.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var network = Network.Main;
			var blocksByHash = LoadBlocksFromDisk(network);
			LoadChain(blocksByHash, network);
		}

		private static void LoadChain(Dictionary<uint256, Block> blocksByHash, Network network)
		{
			var blocksByPrevBlockHash = new Dictionary<uint256, List<Block>>();

			foreach (var blockPair in blocksByHash)
			{
				if (!blocksByPrevBlockHash.ContainsKey(blockPair.Value.Header.HashPrevBlock))
				{
					blocksByPrevBlockHash.Add(blockPair.Value.Header.HashPrevBlock, new List<Block>());
				}

				blocksByPrevBlockHash[blockPair.Value.Header.HashPrevBlock].Add(blockPair.Value);
			}

			//start with block 1, because the genesis block has problems. rut roh
			Block tip = blocksByPrevBlockHash[new uint256("0x00001123368370feb0997f471423e4445be205b9947e7053c762886317274d2a")].First(); //genesis block's hash for mainnet
			List<Block> blockChain = new List<Block>();
			while (tip != null)
			{
				blockChain.Add(tip);

				if (!blocksByPrevBlockHash.ContainsKey(tip.Header.GetHash()))
				{
					break;
				}

				//this fork detection works for 1 block forks. needs enhancement to detect larger ones and still find the longest chain.
				var possibleNewTips = blocksByPrevBlockHash[tip.Header.GetHash()];
				if (possibleNewTips.Count > 1)
				{
					System.Console.WriteLine("Fork Detected at Block {0}", blockChain.Count);
					Block newTip = null;
					foreach (var possibleNewTip in possibleNewTips)
					{
						if (!blocksByPrevBlockHash.ContainsKey(possibleNewTip.Header.GetHash()))
						{
							continue;
						}

						var possibleNewForkTips = blocksByPrevBlockHash[possibleNewTip.Header.GetHash()];
						if (possibleNewForkTips.Count > 0)
						{
							newTip = possibleNewTip;
						}
					}

					if (newTip == null)
					{
						break;
					}
					tip = newTip;
				}
				else
				{
					tip = possibleNewTips[0];
				}
			}

			System.Console.WriteLine(blockChain.Count);

			var dbBlocks = BlockExplorerService.GetAllBlocks();
			for (var index = 0; index < blockChain.Count; index++)
			{
				var block = blockChain[index];

				if (index <= blockChain.Count - 10 && !dbBlocks.ContainsKey(block.GetHash()))
				{
					System.Console.WriteLine(String.Format("adding block {0} to db", index));

					BlockExplorerService.AddBlock(block, network);
				}
			}
			

			/*
			//doesn't work, perhaps because of commented out/incorrect code around genesis block. The genesis block is still bitcoins, not dnotes.
			var network = Network.Main;
			var store = new NBitcoin.BitcoinCore.BlockStore(AppSettings.BlockFolderPath, network);
			store.FileRegex = new Regex(store.FilePrefix + "([0-9]{4,4}).dat");

			ConcurrentChain chain = new ConcurrentChain(network);
			store.SynchronizeChain(chain);

			var index = new IndexedBlockStore(new InMemoryNoSqlRepository(), store);
			index.ReIndex();

			var tip = chain.Tip;
			var tipBlock = index.Get(tip.HashBlock);
			*/
		}

		private static Dictionary<uint256, Block> LoadBlocksFromDisk(Network network)
		{
			var store = new NBitcoin.BitcoinCore.BlockStore(AppSettings.BlockFolderPath, network);
			store.FileRegex = new Regex(store.FilePrefix + "([0-9]{4,4}).dat");

			var diskBlocks = new Dictionary<uint256,Block>();

			int blockIndex = 0;
			foreach (var blockItem in store.Enumerate(false))
			{
				var block = blockItem.Item;
				System.Console.WriteLine(String.Format("block #(on disk): {0}. transaction count: {1}", blockIndex, block.Transactions.Count));

				diskBlocks.Add(block.GetHash(), block);

				blockIndex++;
			}

			return diskBlocks;
		}
	}
}
