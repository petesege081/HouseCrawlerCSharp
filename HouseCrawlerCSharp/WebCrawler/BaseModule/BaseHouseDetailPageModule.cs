using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.WebCrawler.BaseModule;
using System;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.Model
{
	abstract class BaseHouseDetailPageModule : BasePageModule
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

			Watcher.Start();

			//Open page
			Driver.Navigate().GoToUrl(HouseLink);

			Watcher.Stop();
			Timer.Connect = Watcher.ElapsedMilliseconds;
			Watcher.Restart();

			//Wait fo page ready
			WaitForPageLoaded();
			IsHouseExist = CheckHouseExist();
			AfterPageLoadedEvent();

			Watcher.Stop();
			Timer.PageReady = Watcher.ElapsedMilliseconds;

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
