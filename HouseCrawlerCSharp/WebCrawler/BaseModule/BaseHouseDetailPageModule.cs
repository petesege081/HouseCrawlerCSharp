using HouseCrawlerCSharp.WebCrawler.BaseModule;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.Model
{
	abstract class BaseHouseDetailPageModule : BaseWebDriver
	{
		protected string HouseId;
		protected string HouseLink;
		protected bool IsHouseExist;

		/// <summary>
		/// Open house detail page.
		/// </summary>
		/// <param name="houseId"></param>
		/// <returns>If house information is exist, return true.</returns>
		public BaseHouseDetailPageModule GoTo(string houseId) {
			HouseId = houseId;
			HouseLink = GetHouseDetailLink(houseId);

			Driver.Navigate().GoToUrl(HouseLink);

			WaitForPageLoaded();

			IsHouseExist = CheckHouseExist();

			AfterPageLoadedEvent();

			return this;
		}

		/// <summary>
		/// Get house detail page link.
		/// </summary>
		/// <param name="houseId"></param>
		/// <returns>URL string</returns>
		protected abstract string GetHouseDetailLink(string houseId);

		/// <returns>If this case is not exist or archived, return false.</returns>
		protected abstract bool CheckHouseExist();

		/// <summary>
		/// Wait for detail page is ready.
		/// </summary>
		protected abstract void WaitForPageLoaded();

		/// <summary>
		/// Do something after page ready.
		/// </summary>
		protected abstract void AfterPageLoadedEvent();

		/// <summary>
		/// Scan this page and get house information.
		/// </summary>
		/// <returns></returns>
		public abstract HouseInfo GetHouseInfo(Dictionary<string, object> extras = null);
	}
}
