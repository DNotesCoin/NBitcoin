using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DNotes.BlockExplorer.Service;
using Microsoft.AspNetCore.Mvc;
using DNotes.BlockExplorer.Web.Models;
using NBitcoin;

namespace DNotes.BlockExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
	        var model = new BlockListViewModel();
			model.Blocks.Add(new Block());
			model.Blocks.Add(new Block());

			return View(model);
        }

		[Route("/address/{address}")]
	    public IActionResult Address(string address)
	    {
		    var content = "<!DOCTYPE html><html><body><table>";

		    var transactions = BlockExplorerService.GetTransactionsForAddress(address);

		    foreach (var transaction in transactions)
		    {
				content += string.Format("<tr><td class=\"tx\"><a href=\"{0}\">{0}</a></td></tr>", transaction.hashHACK.ToString());
			}
			content += address;

		    content += "</table></body></html>";
			return Content(content, "text/html");
	    }

		public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
