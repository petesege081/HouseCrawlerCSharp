using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Threading;

namespace HouseCrawlerCSharp.Library
{
	class WebDriverHandler
	{
		public RemoteWebDriver WebDriver;
		public WebDriverWait Waiter;
		public IJavaScriptExecutor Js;
		private int Id;

		private static ConcurrentDictionary<int, WebDriverHandler> DriverCollections = new ConcurrentDictionary<int, WebDriverHandler>();
		private static int DriverCounter = 0;

		public void Quit(){
			if (WebDriver == null)
			{
				return;
			}

			try
			{
				WebDriver.Close();
			}
			catch (Exception)
			{

			}

			try
			{
				WebDriver.Quit();
			}
			catch (Exception)
			{

			}

			DriverCollections.TryRemove(Id, out WebDriverHandler retiredVal);
		}

		public static void CloseAllBrowser(){
			foreach (var dic in DriverCollections)
			{
				if(dic.Value != null)
				{
					dic.Value.Quit();
				}
			}
		}

		public static string GetWebDriverFileName()
		{
			string name = null;
			switch (AppConfig.WebDriverOpts.DriverType)
			{
				case WebDriverType.Chrome:
					name = Path.GetFileNameWithoutExtension(AppConfig.WebDriverOpts.ChromeDriverPath);
					break;
				case WebDriverType.Edge:
					name = Path.GetFileNameWithoutExtension(AppConfig.WebDriverOpts.EdgeDriverPath);
					break;
				case WebDriverType.FireFox:
					name = Path.GetFileNameWithoutExtension(AppConfig.WebDriverOpts.FirefoxDriverPath);
					break;
			}

			return name;
		}

		public static WebDriverHandler CreateDefaultHandler(int pageLoadTimeout = 15){
			var handler = new WebDriverHandler
			{
				Id = Interlocked.Increment(ref DriverCounter)
			};

			DriverCollections.TryAdd(handler.Id, handler);

			switch (AppConfig.WebDriverOpts.DriverType)
			{
				case WebDriverType.Chrome:
					var chromeService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(AppConfig.WebDriverOpts.ChromeDriverPath), Path.GetFileName(AppConfig.WebDriverOpts.ChromeDriverPath));
					chromeService.HideCommandPromptWindow = true;

					var chromeOpts = new ChromeOptions
					{
						AcceptInsecureCertificates = true
					};
					chromeOpts.AddArgument("no-sandbox"); //最高權限

					handler.WebDriver = new ChromeDriver(chromeService, chromeOpts);
					break;
				case WebDriverType.Edge:
					var edgeService = EdgeDriverService.CreateDefaultService(Path.GetDirectoryName(AppConfig.WebDriverOpts.EdgeDriverPath), Path.GetFileName(AppConfig.WebDriverOpts.EdgeDriverPath));
					edgeService.HideCommandPromptWindow = true;

					var edgeOpts = new EdgeOptions
					{
						AcceptInsecureCertificates = true
					};
					edgeOpts.AddAdditionalCapability("no-sandbox", true);

					handler.WebDriver = new EdgeDriver(edgeService, edgeOpts);
					break;
				case WebDriverType.FireFox:
					var firefoxService = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(AppConfig.WebDriverOpts.FirefoxDriverPath), Path.GetFileName(AppConfig.WebDriverOpts.FirefoxDriverPath));
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

			handler.WebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
			handler.WebDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
			handler.WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

			var rd = new Random();
			handler.WebDriver.Manage().Window.Position = new Point(rd.Next(0, 20), rd.Next(0, 20));
			handler.WebDriver.Manage().Window.Size = new Size(Math.Max(AppConfig.WebDriverOpts.BrowserWidth, 1024), Math.Max(AppConfig.WebDriverOpts.BrowserHight, 768));


			handler.Waiter = new WebDriverWait(handler.WebDriver, TimeSpan.FromSeconds(10));
			handler.Js = handler.WebDriver;

			return handler;
		}
	}
}
