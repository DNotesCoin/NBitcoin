using System;
using System.Collections.Generic;
using NBitcoin;

namespace DNotes.BlockExplorer.Web.Models
{
    public class BlockListViewModel
    {
		public List<Block> Blocks { get; set; }

	    public BlockListViewModel()
	    {
		    Blocks = new List<Block>();
	    }
    }
}