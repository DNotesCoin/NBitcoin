﻿using System;
using System.Text.RegularExpressions;
using DNotes.BlockExplorer.Service;
using NBitcoin;
using NBitcoin.BitcoinCore;

namespace DNotes.BlockExplorer.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			//var store = new NBitcoin.BitcoinCore.BlockStore("C:\\Users\\jake.byram\\AppData\\Roaming\\Bitcoin\\regtest\\blocks", Network.RegTest);
			var store = new NBitcoin.BitcoinCore.BlockStore("C:\\Users\\jake.byram\\AppData\\Roaming\\DNotes2\\regtest", Network.RegTest);
			//var store = new NBitcoin.BitcoinCore.BlockStore("C:\\Users\\jake.byram\\AppData\\Roaming\\DNotes2\\", Network.Main);
			store.FileRegex = new Regex(store.FilePrefix + "([0-9]{4,4}).dat");

			var dbBlocks = BlockExplorerService.GetAllBlocks();

			int counter = 0;
			foreach (var blockItem in store.Enumerate(false))
			{
				var block = blockItem.Item;
				System.Console.WriteLine(block.Transactions.Count);
				
				if (!dbBlocks.ContainsKey(block.GetHash()))
				{
					BlockExplorerService.AddBlock(block);
				}
				

				if (block.AddressBalances.Count > 0)
				{
					System.Console.WriteLine("yay");
				}

				counter++;
				if (counter > 10)
				{
					return;
				}
			}

			return;
			/*
			System.Console.WriteLine("Hello World!");
			var store = new NBitcoin.BitcoinCore.BlockStore("C:\\Users\\jake.byram\\AppData\\Roaming\\Bitcoin\\regtest\\blocks", Network.RegTest);
			ConcurrentChain chain = new ConcurrentChain(Network.RegTest);
			store.SynchronizeChain(chain);

			//Use chain here (chain keep just the headers in memory)


			var index = new IndexedBlockStore(new InMemoryNoSqlRepository(), store);
			index.ReIndex();

			var tip = chain.Tip;
			var tipBlock = index.Get(tip.HashBlock);
			*/

		}
	}
}
