using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace HouseCrawlerCSharp.Library
{
	class WebDriverHandler
	{
		public RemoteWebDriver WebDriver;
		public WebDriverWait Waiter;
		public IJavaScriptExecutor Js;

		public static string GetWebDriverFileName()
		{
			string name = null;
			var type = (WebDriverType)Enum.Parse(typeof(WebDriverType), CrawlerConfig.Config["WebDriverOptions:DriverType"]);
			switch (type)
			{
				case WebDriverType.Chrome:
					name = Path.GetFileNameWithoutExtension(CrawlerConfig.Config["WebDriverOptions:ChromeDriverPath"]);
					break;
				case WebDriverType.Edge:
					name = Path.GetFileNameWithoutExtension(CrawlerConfig.Config["WebDriverOptions:EdgeDriverPath"]);
					break;
				case WebDriverType.FireFox:
					name = Path.GetFileNameWithoutExtension(CrawlerConfig.Config["WebDriverOptions:FirefoxDriverPath"]);
					break;
			}

			return name;
		}

		public static WebDriverHandler CreateDefaultHandler(){
			var handler = new WebDriverHandler();
			
			var type = (WebDriverType) Enum.Parse(typeof(WebDriverType), CrawlerConfig.Config["WebDriverOptions:DriverType"]);
			switch (type)
			{
				case WebDriverType.Chrome:
					var chromeService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(CrawlerConfig.Config["WebDriverOptions:ChromeDriverPath"]), Path.GetFileName(CrawlerConfig.Config["WebDriverOptions:ChromeDriverPath"]));
					chromeService.HideCommandPromptWindow = true;

					var chromeOpts = new ChromeOptions
					{
						AcceptInsecureCertificates = true
					};
					chromeOpts.AddArgument("no-sandbox"); //最高權限
					chromeOpts.AddArgument("window-size=1024,768"); //瀏覽器大小

					handler.WebDriver = new ChromeDriver(chromeService, chromeOpts);
					break;
				case WebDriverType.Edge:
					var edgeService = EdgeDriverService.CreateDefaultService(Path.GetDirectoryName(CrawlerConfig.Config["WebDriverOptions:EdgeDriverPath"]), Path.GetFileName(CrawlerConfig.Config["WebDriverOptions:EdgeDriverPath"]));
					edgeService.HideCommandPromptWindow = true;

					var edgeOpts = new EdgeOptions
					{
						AcceptInsecureCertificates = true
					};
					edgeOpts.AddAdditionalCapability("no-sandbox", true);
					edgeOpts.AddAdditionalCapability("window-size", "1024,768");

					handler.WebDriver = new EdgeDriver(edgeService, edgeOpts);
					break;
				case WebDriverType.FireFox:
					var firefoxService = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(CrawlerConfig.Config["WebDriverOptions:FirefoxDriverPath"]), Path.GetFileName(CrawlerConfig.Config["WebDriverOptions:FirefoxDriverPath"]));
					firefoxService.Host = "::1"; //使用IPv6以提升速度
					firefoxService.HideCommandPromptWindow = true;

					var opts = new FirefoxOptions
					{
						AcceptInsecureCertificates = true
					};
					opts.AddArgument("no-sandbox");

					handler.WebDriver = new FirefoxDriver(firefoxService, opts);
					break;
			}

			handler.WebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
			handler.WebDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
			handler.WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

			handler.Waiter = new WebDriverWait(handler.WebDriver, TimeSpan.FromSeconds(10));
			handler.Js = handler.WebDriver;

			return handler;
		}
	}

	enum WebDriverType : int
	{
		Chrome = 1,
		Edge = 2,
		FireFox = 3
	}
}
