using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace HouseCrawlerCSharp.WebCrawler.BaseModule
{
	abstract class BaseWebDriver
	{
		protected IWebDriver Driver;
		protected WebDriverWait Waiter;
		protected IJavaScriptExecutor Js;

		public virtual void SetWebDriver(IWebDriver driver, WebDriverWait waiter, IJavaScriptExecutor js)
		{
			Driver = driver;
			Waiter = waiter;
			Js = js;
		}
	}
}
