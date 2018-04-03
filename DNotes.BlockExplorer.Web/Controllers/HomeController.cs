using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
