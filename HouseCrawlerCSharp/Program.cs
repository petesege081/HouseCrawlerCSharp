using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.WebCrawler;
using NLog;
using OpenQA.Selenium;
using System;
using System.Diagnostics;

namespace HouseCrawlerCSharp
{
	class Program
	{
		static void Main()
		{
			var Logger = LogManager.GetLogger("Default");
			var errorLogger = LogManager.GetLogger("CrawlerError");

			//取得模組類型
			var type = (CrawlerModuleType)Enum.Parse(typeof(CrawlerModuleType), CrawlerConfig.Config["CrawlerOptions:ModuleType"]);

			//是否最上層執行
			if (bool.Parse(CrawlerConfig.Config["ApplicationOptions:OnTop"])) {
				ConsoleWindowHandler.SetConsoleWindowOnTop();
			}

			//調整Console視窗大小並置於螢幕右側
			var width = int.Parse(CrawlerConfig.Config["ApplicationOptions:WindowWidth"]);
			var hight = int.Parse(CrawlerConfig.Config["ApplicationOptions:WindowHight"]);
			width = width > Console.BufferWidth ? Console.BufferWidth : width;
			hight = hight > 60 ? 60 : hight;
			Console.SetWindowSize(width, hight);
			ConsoleWindowHandler.SetConsoleWindowPosition((int)type - 1);

			//產生要執行的模組
			BaseCrawlerModule crawlerModule = null;
			switch (type)
			{
				case CrawlerModuleType._591:
					crawlerModule = new Module591();
					Console.Title = "591 House Crawler";
					break;
				case CrawlerModuleType.Sinyi:
					Console.Title = "Sinyi House Crawler";
					crawlerModule = new ModuleSinyi();
					break;
				case CrawlerModuleType.YungChing:
					Console.Title = "YungChing House Crawler";
					crawlerModule = new ModuleYungChing();
					break;
				default:
					Logger.Error("Module type is invalid.");
					return;
			}
			crawlerModule.SetFolder(CrawlerConfig.Config["CrawlerOptions:WorkFolder"]);

			//關閉程式時同時關閉WebDriver
			Console.TreatControlCAsInput = true;
			AppDomain.CurrentDomain.ProcessExit += new EventHandler((s, e) =>
			{
				KillAllDriverProcess();
			});

			// 執行模組, 如果WebDriver有問題, 則從記錄點重新開始
			while (true){
				try
				{
					crawlerModule.StartProcess();
					break;
				}
				catch (WebDriverException ex)
				{
					Logger.Error($"WebDriver is abnormal, restarting the process...\n> {ex.Message}");
				}
				catch(Exception ex)
				{
					Logger.Error($"Unexpected exception!!!\n{ex.Message}");
					errorLogger.Error($"System| Unexpected exception!!!\n>{ex}");
					KillAllDriverProcess();
					throw ex;
				}
			}

			Console.WriteLine("");
			Logger.Info("All done!");
			Console.WriteLine("");

			Console.WriteLine("Press ENTER to close...");
			var keyInfo = Console.ReadKey();
			while (keyInfo.Key != ConsoleKey.Enter)
			{
				keyInfo = Console.ReadKey();
			}
		}

		static void KillAllDriverProcess(){
			var driverName = WebDriverHandler.GetWebDriverFileName();
			var currProcessId = Process.GetCurrentProcess().Id;
			foreach (var process in Process.GetProcessesByName(driverName))
			{
				if (ParentProcessUtilities.GetParentProcess(process.Handle).Id == currProcessId)
				{
					process.Kill();
				}
			}
		}
	}
}
