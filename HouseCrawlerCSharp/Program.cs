using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.WebCrawler;
using NLog;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace HouseCrawlerCSharp
{
	class Program
	{
		static void Main()
		{
			//Debug();
			//return;

			var Logger = LogManager.GetLogger("Default");

			//產生要執行的模組
			BaseCrawlerModule crawlerModule = null;
			switch(CrawlerConfig.Config["CrawlerOptions:ModuleType"])
			{
				case "1":
					crawlerModule = new Module591();
					break;
				case "2":
					crawlerModule = new ModuleSinyi();
					break;
				case "3":
					crawlerModule = new ModuleYungChing();
					break;
				default:
					Console.WriteLine("Module type is invalid.");
					return;
			}
			crawlerModule.SetFolder(CrawlerConfig.Config["CrawlerOptions:WorkFolder"]);

			//關閉程式時同時關閉瀏覽器
			Console.TreatControlCAsInput = true;
			AppDomain.CurrentDomain.ProcessExit += new EventHandler((s, e) =>
			{
				crawlerModule.CloseBrowser();
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
					crawlerModule.CloseBrowser();
					Logger.Error($"WebDriver is abnormal, restarting the process...\n> {ex.Message}");
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

		static void Debug(){
			var processData = FileHelper.ReadProcessData(@"D:\Code\_HouseData2\YungChing");
			var houseId = "4945203";

			var info = new ModuleYungChing().InitWebDriver().HouseDetailPage.GoTo(houseId).GetHouseInfo(processData.HouseList.FirstOrDefault(o => o.HouseId == houseId).Extras); ;

			var gg = 0;
		}
	}
}
