using HouseCrawlerCSharp.Library;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace HouseCrawlerCSharp.WebCrawler.BaseModule
{
	abstract class BaseWebDriver
	{
		protected IWebDriver Driver;
		protected WebDriverWait Waiter;
		protected IJavaScriptExecutor Js;

		public virtual void InitWebDriverHandler()
		{
			var handler = WebDriverHandler.CreateDefaultHandler();
			Driver = handler.WebDriver;
			Waiter = handler.Waiter;
			Js = handler.Js;
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
}