using HouseCrawlerCSharp.Library;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;

namespace HouseCrawlerCSharp.WebCrawler.BaseModule
{
	abstract class BasePageModule
	{
		protected IWebDriver Driver;
		protected WebDriverWait Waiter;
		protected IJavaScriptExecutor Js;

		protected Timer Timer = new Timer();
		protected Stopwatch Watcher;

		public Timer GetTimer(){
			return Timer;
		}

		public virtual void InitWebDriverHandler(int pageLoadTimeout = 15)
		{
			var handler = WebDriverHandler.CreateDefaultHandler(pageLoadTimeout);
			Driver = handler.WebDriver;
			Waiter = handler.Waiter;
			Js = handler.Js;

			Watcher = new Stopwatch();
		}

		public virtual void SetWebDriverHandler(WebDriverHandler handler)
		{
			Driver = handler.WebDriver;
			Waiter = handler.Waiter;
			Js = handler.Js;
		}

		/// <summary>
		/// Close browser & webdriver.exe
		/// </summary>
		public virtual void Quit()
		{
			if(Driver == null)
			{
				return;
			}

			try
			{
				Driver.Close();
			}
			catch (Exception)
			{

			}
			finally
			{
				Driver.Quit();
			}
		}
	}

	class Timer
	{
		public double Connect;
		public double PageReady;
		public double DataCapture;
	}
}