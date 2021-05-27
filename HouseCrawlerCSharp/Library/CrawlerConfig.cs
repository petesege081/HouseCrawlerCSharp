using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseCrawlerCSharp.Library
{
	class CrawlerConfig
	{
		public static IConfiguration Config = new ConfigurationBuilder()
		   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
		   .Build();
	}

	enum CrawlerModuleType : int
	{
		_591 = 1,
		Sinyi = 2,
		YungChing = 3
	}
}
