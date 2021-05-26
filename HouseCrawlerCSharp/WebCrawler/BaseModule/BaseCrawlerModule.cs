using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler.BaseModule;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace HouseCrawlerCSharp.WebCrawler
{
	abstract class BaseCrawlerModule
	{
		private readonly Logger Logger = LogManager.GetLogger("Default");
		private readonly Logger InfoLogger = LogManager.GetLogger("InfoError");
		private readonly Logger ImageLogger = LogManager.GetLogger("ImageError");

		protected CrawlerProcessData ProcessData;
		private ChromeDriver WebDriver;

		public BaseHouseListPageModule HouseListPage;
		public BaseHouseDetailPageModule HouseDetailPage;

		private string WorkFolder = "";
		private readonly string ModuleType;

		protected abstract CrawlerProcessData GetDefaultProcessData();
		protected abstract string GetModuleType();
		protected abstract BaseHouseListPageModule InitHouseListPage();
		protected abstract BaseHouseDetailPageModule InitHouseDetailPage();

		public BaseCrawlerModule()
		{
			ModuleType = GetModuleType();

			InitHouseListPage();
			InitHouseDetailPage();
		}

		///<summary>
		///Set the data output folder.
		///</summary>
		///<param name="folder"></param>
		public BaseCrawlerModule SetFolder(string folder)
		{
			if (string.IsNullOrEmpty(folder.Trim()))
			{
				WorkFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ModuleType);
			}
			else
			{
				WorkFolder = Path.Combine(folder, ModuleType);
			}
			
			LogManager.Configuration.Variables["WorkFolder"] = WorkFolder;

			return this;
		}

		///<summary>
		///Set the sort option for searching house list.
		///</summary>
		///<param name="orderBy"></param>
		///<returns></returns>
		public BaseCrawlerModule SetOrderBy(int orderBy)
		{
			HouseListPage.OrderBy = orderBy;
			return this;
		}

		public BaseCrawlerModule InitWebDriver(){
			var service = ChromeDriverService.CreateDefaultService(@"_WebDriver", "chromedriver.exe");
			service.HideCommandPromptWindow = true;

			WebDriver = new ChromeDriver(service); //會開啟瀏覽器
			WebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
			WebDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
			WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

			var waiter = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(10));
			var js = (IJavaScriptExecutor)WebDriver;

			HouseListPage.SetWebDriver(WebDriver, waiter, js);
			HouseDetailPage.SetWebDriver(WebDriver, waiter, js);

			return this;
		}

		public BaseCrawlerModule CloseBrowser()
		{
			WebDriver.Quit();

			return this;
		}

		public void StartProcess()
		{
			//啟動WebDriver
			InitWebDriver();

			//嘗試取得ProcessData
			ProcessData = FileHelper.ReadProcessData(WorkFolder);

			//ProcessData若為NULL, 則為全新的開始
			if (ProcessData == null)
			{
				ProcessData = GetDefaultProcessData();
				FileHelper.SaveProcessData(WorkFolder, ProcessData);
			}

			//Create work folder
			if (!Directory.Exists(WorkFolder))
			{
				Directory.CreateDirectory(WorkFolder);
			}

			var tmpHouseInfos = new List<HouseInfo>(); //未寫入CSV的暫存資料

			//產生CSV檔
			var csvHandler = new CsvFileHandler<HouseInfo>(Path.Combine(WorkFolder, "Output.csv"));
			if (!csvHandler.IsExist())
			{
				csvHandler.CreateFileWithHeader(HouseInfo.Headers);
			}

			//依地區進行檢索
			for (var i = 0; i < ProcessData.Regions.Count; i++)
			{
				if (ProcessData.Regions[i].Status == RegionStatus.SKIP)
				{
					continue;
				}

				//跳過已完成的地區
				if (ProcessData.Regions[i].Status == RegionStatus.DONE)
				{
					Logger.Info($"{ProcessData.Regions[i].Name} > Done");
					continue;
				}

				//若該地區處於佇列中, 開始搜尋區域內的House ID
				if (ProcessData.Regions[i].Status == RegionStatus.QUEUE)
				{
					Logger.Info($@"{ProcessData.Regions[i].Name} > Start searching...");

					ProcessData.HouseList = new List<HouseListItem>();
					ProcessData.Cursor = 0;

					try
					{
						//Open house list page
						HouseListPage.GoTo(ProcessData.Regions[i].SiteKey);

						while (true)
						{
							ProcessData.HouseList.AddRange(HouseListPage.GetHouseList());

							Logger.Trace($"Region: {ProcessData.Regions[i].Name}, Page: {HouseListPage.PageIndex}, Count: {ProcessData.HouseList.Count}");

							Thread.Sleep(500);

							if (!HouseListPage.HasNextPage)
							{
								break;
							}

							HouseListPage.NextPage();
						}

						//Remove duplicate house
						ProcessData.HouseList = ProcessData.HouseList.GroupBy(x => x.HouseId).Select(y => y.First()).ToList();
					}
					catch (WebDriverException ex)
					{
						InfoLogger.Error($"HouseList|{ProcessData.Regions[i].Name}\n{ex}");
						throw new WebDriverException(ex.Message, ex);
					}
					catch (Exception ex)
					{
						Logger.Error($"{ProcessData.Regions[i].Name} > Search house list failed!\n> {ex.Message}");
						InfoLogger.Error($"HouseList|{ProcessData.Regions[i].Name}\n{ex}");
					}

					ProcessData.Regions[i].Status = RegionStatus.IN_PROCESS;

					//儲存當前進度資料
					FileHelper.SaveProcessData(WorkFolder, ProcessData);
				}
				else
				{
					Logger.Info($"{ProcessData.Regions[i].Name} > Continue the unfinished parts...");
				}

				//Get house info
				for (; ProcessData.Cursor < ProcessData.HouseList.Count;)
				{
					var houseId = ProcessData.HouseList[ProcessData.Cursor].HouseId;

					try
					{
						//Open house detail page & get house info
						var info = HouseDetailPage.GoTo(houseId).GetHouseInfo(ProcessData.HouseList[ProcessData.Cursor].Extras);

						if (info != null)
						{
							tmpHouseInfos.Add(info);
							DownloadHousePhotos(info);

							Logger.Trace($"{houseId} > Get info successfully. ({ProcessData.Cursor + 1}/{ProcessData.HouseList.Count})");
						}
						else
						{
							Logger.Warn($"{houseId} > Get info failed!. ({ProcessData.Cursor + 1}/{ProcessData.HouseList.Count})\n> This case is not exist or archived.");
							InfoLogger.Warn($"HouseDetail|{houseId}\nThis case is not exist or archived.");
						}
					}
					catch (WebDriverException ex)
					{
						InfoLogger.Error($"HouseDetail|{houseId}\n{ex}");
						throw new WebDriverException(ex.Message, ex);
					}
					catch (Exception ex)
					{
						//取資料失敗時, 立即儲存當前進度
						ProcessData.FailedCases.Add(new FailedCase { HouseItem = ProcessData.HouseList[ProcessData.Cursor] });
						FileHelper.SaveProcessData(WorkFolder, ProcessData);

						Logger.Error($"{houseId} > Get info failed!. ({ProcessData.Cursor + 1}/{ProcessData.HouseList.Count})\n> {ex.Message}");
						InfoLogger.Error($"HouseDetail|{houseId}\n{ex}");
					}

					ProcessData.Cursor++;

					//每取得10筆資料或已完成此區域便寫入CSV
					if (tmpHouseInfos.Count >= 10 || ProcessData.Cursor == ProcessData.HouseList.Count - 1)
					{
						//寫入CSV檔
						WriteToCsv(csvHandler, tmpHouseInfos);

						//清空暫存資料
						tmpHouseInfos.Clear();

						//儲存當前進度資料
						FileHelper.SaveProcessData(WorkFolder, ProcessData);
					}
				}

				//嘗試取得曾經失敗的房屋資料
				if(ProcessData.FailedCases.Count > 0)
				{
					RetryFailedCases(csvHandler);
				}

				//儲存當前進度資料
				ProcessData.Regions[i].Status = RegionStatus.DONE;
				ProcessData.HouseList.Clear();
				ProcessData.Cursor = 0;
				FileHelper.SaveProcessData(WorkFolder, ProcessData);
			}
		}

		private HouseInfo DownloadHousePhotos(HouseInfo info)
		{
			info.PhotoDirectory = Path.Combine(WorkFolder, "Photos", info.Id);

			//Download photos
			if (Directory.Exists(info.PhotoDirectory))
			{
				Directory.Delete(info.PhotoDirectory, true);
			}
			Directory.CreateDirectory(info.PhotoDirectory);

			for (var imgInex = 0; imgInex < info.PhotoLinks.Count; imgInex++)
			{
				try
				{
					var dl = new ImageDownloader();
					dl.Download(info.PhotoLinks[imgInex]);
					dl.SaveImage(Path.Combine(info.PhotoDirectory, $"{imgInex + 1}.jpg"), ImageFormat.Jpeg);
				}
				catch (Exception ex)
				{
					Logger.Error($"{info.Id} > Download image failed.\n> URL: {info.PhotoLinks[imgInex]}\n> {ex.Message}");
					ImageLogger.Error($"{info.Id}\n{info.PhotoLinks[imgInex]}\n{ex}");
				}
			}

			return info;
		}

		private void WriteToCsv(CsvFileHandler<HouseInfo> csvHandler, List<HouseInfo> infos)
		{
			while (true)
			{
				if (csvHandler.IsLocked())
				{
					Logger.Warn("CSV file is currently in use, please close the related programs!");
					Thread.Sleep(5000);
				}
				else
				{
					csvHandler.AppendToFile(infos);

					Logger.Info("Write data to CSV file successfully.");
					break;
				}
			}
		}

		private void RetryFailedCases(CsvFileHandler<HouseInfo> csvHandler)
		{
			Logger.Info($@"Retry searching failed cases...");

			var retryInfos = new List<HouseInfo>();
			for (var cIndex = ProcessData.FailedCases.Count - 1; cIndex >= 0; cIndex--)
			{
				ProcessData.FailedCases[cIndex].RetryCount++;

				var houseId = ProcessData.FailedCases[cIndex].HouseItem.HouseId;
				var retryCount = ProcessData.FailedCases[cIndex].RetryCount;

				try
				{
					var info = HouseDetailPage.GoTo(houseId).GetHouseInfo(ProcessData.FailedCases[cIndex].HouseItem.Extras);

					if (info != null)
					{
						retryInfos.Add(info);
						DownloadHousePhotos(info);
						Logger.Trace($"{houseId} > Get info successfully. (Retry: {retryCount})");
					}
					else
					{
						Logger.Warn($"{houseId} > Get info failed!. (Retry: {retryCount})\n> This case is not exist or archived.");
					}

					//從失敗案例中移除
					ProcessData.FailedCases.RemoveAt(cIndex);
				}
				catch (WebDriverException ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Logger.Error($"{houseId} > Get info failed!. (Retry: {retryCount})\n> {ex.Message}");
					InfoLogger.Error($"HouseDetail|{houseId}|Retry={retryCount}\n{ex}");
				}
			}

			//寫入CSV檔
			if (retryInfos.Count > 0)
			{
				WriteToCsv(csvHandler, retryInfos);
			}
		}
	}
}

	
