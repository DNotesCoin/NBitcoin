using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DNotes.Infrastructure
{
	public class AppSettings
	{
		private static IConfiguration Configuration { get; set; }

		static AppSettings()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");

			Configuration = builder.Build();
		}

		public static string BlockFolderPath => Configuration["AppSettings:BlockFolderPath"];
		public static string BlockExplorerConnectionString => Configuration["ConnectionStrings:DNotes.BlockExplorer"];
	}
}
