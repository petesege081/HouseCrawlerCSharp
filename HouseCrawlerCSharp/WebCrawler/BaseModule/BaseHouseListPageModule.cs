﻿using HouseCrawlerCSharp.WebCrawler.BaseModule;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.Model
{
	abstract class BaseHouseListPageModule : BaseWebDriver
	{
		public int PageIndex = 0;
		public bool HasNextPage;
		public int OrderBy = HouseOrderBy.PUBLISH_DESC; //預設排序為刊登日期新到舊

		public BaseHouseListPageModule GoTo(string regionKey)
		{
			Driver.Navigate().GoToUrl(GetHouseListLink(regionKey, OrderBy));
			PageIndex = 0;
			WaitForPageLoaded();
			AfterPageLoadedEvent();

			return this;
		}

		/// <summary>
		/// Get house list page link.
		/// </summary>
		/// <param name="houseId"></param>
		/// <returns>URL string</returns>
		protected abstract string GetHouseListLink(string regionKey, int? orderBy);

		/// <summary>
		/// Wait for list page is ready.
		/// </summary>
		protected abstract void WaitForPageLoaded();

		/// <summary>
		/// Do something after page ready.
		/// </summary>
		protected abstract void AfterPageLoadedEvent();

		/// <summary>
		/// Go to the next page.
		/// </summary>
		public abstract BaseHouseListPageModule NextPage();

		/// <summary>
		/// Get house count in this area.
		/// </summary>
		/// <returns></returns>
		protected abstract int GetHouseCount();

		/// <summary>
		/// Scan all house ID in house list
		/// </summary>
		/// <returns></returns>
		public abstract List<HouseListItem> GetHouseList();
	}
}
