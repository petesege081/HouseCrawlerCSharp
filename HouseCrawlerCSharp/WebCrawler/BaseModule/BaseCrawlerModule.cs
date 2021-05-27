﻿using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler.BaseModule;
using NLog;
using OpenQA.Selenium;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HouseCrawlerCSharp.WebCrawler
{
	abstract class BaseCrawlerModule
	{
		private readonly Logger Logger = LogManager.GetLogger("Default");
		private readonly Logger errorLogger = LogManager.GetLogger("CrawlerError");
		private readonly Logger ImageLogger = LogManager.GetLogger("ImageError");

		protected CrawlerProcessData ProcessData;

		private string WorkFolder = "";
		private readonly string ModuleType;
		private int ListOrderBy;

		protected abstract CrawlerProcessData GetDefaultProcessData();
		protected abstract string GetModuleType();
		public abstract BaseHouseListPageModule CreateHouseListPage();
		public abstract BaseHouseDetailPageModule CreateHouseDetailPage();

		public BaseCrawlerModule()
		{
			ModuleType = GetModuleType();
			LogManager.Configuration.Variables["ModuleType"] = ModuleType;
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
			ListOrderBy = orderBy;
			return this;
		}

		public void StartProcess()
		{
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

					var page = CreateHouseListPage();

					try
					{
						//Open house list page
						page.InitWebDriverHandler();
						page.GoTo(ProcessData.Regions[i].SiteKey);

						while (true)
						{
							ProcessData.HouseList.AddRange(page.GetHouseList());

							Logger.Trace($"Region: {ProcessData.Regions[i].Name}, Page: {page.PageIndex}, Count: {ProcessData.HouseList.Count}");

							Thread.Sleep(500);

							if (!page.HasNextPage)
							{
								break;
							}

							page.NextPage();
						}

						//Remove duplicate house
						ProcessData.HouseList = ProcessData.HouseList.GroupBy(x => x.HouseId).Select(y => y.First()).ToList();
					}
					catch (WebDriverException ex)
					{
						page.Quit();

						errorLogger.Error($"HouseList|{ProcessData.Regions[i].Name}\n{ex}");
						throw new WebDriverException(ex.Message, ex);
					}
					catch (Exception ex)
					{
						Logger.Error($"{ProcessData.Regions[i].Name} > Search house list failed!\n> {ex.Message}");
						errorLogger.Error($"HouseList|{ProcessData.Regions[i].Name}\n{ex}");
					}
					finally
					{
						page.Quit();
					}

					ProcessData.Regions[i].Status = RegionStatus.IN_PROCESS;

					//儲存當前進度資料
					FileHelper.SaveProcessData(WorkFolder, ProcessData);
				}
				else
				{
					Logger.Info($"{ProcessData.Regions[i].Name} > Continue the unfinished parts...");
				}

				//Get house infos
				for (; ProcessData.Cursor < ProcessData.HouseList.Count;)
				{
					//一次處理30筆
					var poolCount = 30;

					//以多線程方式取得資料
					var infos = GetHouseInfosByParallel(poolCount);

					ProcessData.Cursor += poolCount;

					//寫入CSV檔
					WriteToCsv(csvHandler, infos);

					//儲存當前進度資料
					FileHelper.SaveProcessData(WorkFolder, ProcessData);
				}

				//嘗試取得曾經失敗的房屋資料
				if (ProcessData.FailedCases.Count > 0)
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


		/// <summary>
		/// Get house info by muti-threads
		/// </summary>
		/// <param name="poolCount"></param>
		/// <returns></returns>
		private List<HouseInfo> GetHouseInfosByParallel(int poolCount)
		{
			var successInfos = new ConcurrentBag<HouseInfo>();
			var failedCases = new ConcurrentBag<FailedCase>();
			var counter = 0;

			var options = new ParallelOptions
			{
				MaxDegreeOfParallelism = int.Parse(CrawlerConfig.Config["WebDriverOptions:MaxThreads"])
			};

			var size = ProcessData.Cursor + poolCount > ProcessData.HouseList.Count ? ProcessData.HouseList.Count - ProcessData.Cursor : poolCount;
			Parallel.For(0, size, options, (i) =>
			{
				var houseItem = ProcessData.HouseList[ProcessData.Cursor + i];
				var page = CreateHouseDetailPage();
				
				try
				{
					page.InitWebDriverHandler();

					//Open house detail page & get house info
					var info = page.GoTo(houseItem.HouseId).GetHouseInfo(ProcessData.HouseList[ProcessData.Cursor + i].Extras);

					if (info != null)
					{
						info.CreatTime = DateTime.Now;
						successInfos.Add(info);
						DownloadHousePhotos(info);

						Logger.Trace($"{houseItem.HouseId} > Get info successfully. ({ProcessData.Cursor + Interlocked.Increment(ref counter)}/{ProcessData.HouseList.Count})");
					}
					else
					{
						Logger.Warn($"{houseItem.HouseId} > Get info failed!. ({ProcessData.Cursor + Interlocked.Increment(ref counter)}/{ProcessData.HouseList.Count})\n> This case is not exist or archived.");
						errorLogger.Warn($"HouseDetail|{houseItem.HouseId}\nThis case is not exist or archived.");
					}
				}
				catch (Exception ex)
				{
					page.Quit();

					failedCases.Add(new FailedCase { HouseItem = ProcessData.HouseList[ProcessData.Cursor + i] });

					Logger.Error($"{houseItem.HouseId} > Get info failed!. ({ProcessData.Cursor + Interlocked.Increment(ref counter)}/{ProcessData.HouseList.Count})\n> {ex.Message}");
					errorLogger.Error($"HouseDetail|{houseItem.HouseId}\n{ex}");
				}
				finally
				{
					page.Quit();
				}
			});

			ProcessData.FailedCases.AddRange(failedCases.ToList());

			return successInfos.ToList();
		}


		/// <summary>
		/// Download house photos
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Write infos to CSV when CSV is not locked.
		/// </summary>
		/// <param name="csvHandler"></param>
		/// <param name="infos"></param>
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


		/// <summary>
		/// Re-scan house info that faild before.
		/// </summary>
		/// <param name="csvHandler"></param>
		private void RetryFailedCases(CsvFileHandler<HouseInfo> csvHandler)
		{
			Logger.Info($@"Retry searching failed cases...");

			var failedCases = new ConcurrentBag<FailedCase>();
			var successInfos = new ConcurrentBag<HouseInfo>();

			Parallel.For(0, ProcessData.FailedCases.Count, (i) =>
			{
				var houseId = ProcessData.FailedCases[i].HouseItem.HouseId;
				var retryCount = ProcessData.FailedCases[i].RetryCount + 1;
				var page = CreateHouseDetailPage();
				
				try
				{
					page.InitWebDriverHandler();

					var info = page.GoTo(houseId).GetHouseInfo(ProcessData.FailedCases[i].HouseItem.Extras);

					if (info != null)
					{
						info.CreatTime = DateTime.Now;
						successInfos.Add(info);
						DownloadHousePhotos(info);
						Logger.Trace($"{houseId} > Get info successfully. (Retry: {retryCount})");
					}
					else
					{
						Logger.Warn($"{houseId} > Get info failed!. (Retry: {retryCount})\n> This case is not exist or archived.");
					}
				}
				catch (Exception ex)
				{
					page.Quit();

					failedCases.Add(new FailedCase
					{
						HouseItem = ProcessData.FailedCases[i].HouseItem,
						RetryCount = retryCount
					});

					Logger.Error($"{houseId} > Get info failed!. (Retry: {retryCount})\n> {ex.Message}");
					errorLogger.Error($"HouseDetail|{houseId}|Retry={retryCount}\n{ex}");
				}
				finally
				{
					page.Quit();
				}
			});

			ProcessData.FailedCases = failedCases.ToList();

			//寫入CSV檔
			if (successInfos.Count > 0)
			{
				WriteToCsv(csvHandler, successInfos.ToList());
			}
		}
	}
}

	
