using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HouseCrawlerCSharp
{
	class Program
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
		private const int HWND_TOPMOST = -1;
		private const int SWP_NOMOVE = 0x0002;
		private const int SWP_NOSIZE = 0x0001;

		static void Main()
		{
			//最上層執行
			var hWnd = Process.GetCurrentProcess().MainWindowHandle;
			SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

			var Logger = LogManager.GetLogger("Default");
			var errorLogger = LogManager.GetLogger("CrawlerError");

			//產生要執行的模組
			BaseCrawlerModule crawlerModule = null;
			var type = (CrawlerModuleType)Enum.Parse(typeof(CrawlerModuleType), CrawlerConfig.Config["CrawlerOptions:ModuleType"]);
			switch (type)
			{
				case CrawlerModuleType._591:
					crawlerModule = new Module591();
					break;
				case CrawlerModuleType.Sinyi:
					crawlerModule = new ModuleSinyi();
					break;
				case CrawlerModuleType.YungChing:
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
