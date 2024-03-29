﻿using HouseCrawlerCSharp.Model;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.WebCrawler._591
{
	class HouseListPage : BaseHouseListPageModule
	{
		protected override string GetHouseListLink(string regionKey, int page, string totalRows, int? orderBy)
		{
			var orderStr = orderBy switch
			{
				HouseOrderBy.PUBLISH_DESC => "posttime_desc",
				//HouseOrderBy.PUBLISH_ASC => "posttime_asc",
				_ => "",
			};

			var url = $"https://sale.591.com.tw/?shType=list&regionid={regionKey}";
			if(!string.IsNullOrWhiteSpace(orderStr)) {
				url += $"&order={orderStr}";
			}

			if(page > 1){
				url += $"&firstRow={(page - 1) * 30}&totalRows={totalRows}";
			}

			return url;
		}
				
		protected override void WaitForPageLoaded()
		{
			Js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

			//頁面切換完成表示已載入資料
			Waiter.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".pages .pageCurrent")));
			Waiter.Until(cond => {
				var index = int.Parse(Driver.FindElement(By.CssSelector(".pages .pageCurrent")).Text);

				return index != PageIndex;
			});
		}

		protected override void AfterPageLoadedEvent()
		{
			//Close help tip
			var items = Driver.FindElements(By.CssSelector(".house-switch div"));
			foreach (var item in items)
			{
				if (item.GetAttribute("class") == "tips-popbox-img" && item.Displayed)
				{
					item.Click();
					break;
				}
			}

			//處理無資料的情況
			if(Driver.FindElement(By.CssSelector(".noHouse-tips")).Displayed)
			{
				PageIndex = 1;
				HasNextPage = false;
				return;
			}

			//Change current page index
			PageIndex = int.Parse(Driver.FindElement(By.CssSelector(".pages .pageCurrent")).Text);

			//Check current page is last page
			try
			{
				var nextBtn = Driver.FindElement(By.CssSelector(".pages .pageNext"));
				HasNextPage = !string.IsNullOrEmpty(nextBtn.GetAttribute("href"));
			}
			catch (NoSuchElementException)
			{
				HasNextPage = false;
			}
		}

		public override BaseHouseListPageModule NextPage()
		{
			Driver.FindElement(By.CssSelector(".pages .pageNext")).Click();

			//Wait for content load
			WaitForPageLoaded();
			AfterPageLoadedEvent();

			return this;
		}

		public override int GetHouseCount()
		{
			if(HouseCount < 0){
				var totalStr = Driver.FindElement(By.CssSelector(".houseList-head-title")).Text;
				var match = Regex.Match(totalStr, @"(\d+,)*\d+");
				HouseCount = match.Success ? int.Parse(match.Groups[0].Value) : 0;
			}

			return HouseCount;
		}

		public override List<HouseListItem> GetHouseList()
		{
			Watcher.Restart();

			var houseList = new List<HouseListItem>();

			var houseBody = Driver.FindElement(By.CssSelector(".houseList-body"));
			if(string.IsNullOrWhiteSpace(houseBody.GetAttribute("innerHTML")))
			{
				return houseList;
			}

			var cards = houseBody.FindElements(By.CssSelector("div[data-bind]"));
			foreach (var card in cards)
			{
				houseList.Add(new HouseListItem { HouseId = card.GetAttribute("data-bind") } );
			}

			Watcher.Stop();
			Timer.DataCapture = Watcher.ElapsedMilliseconds;

			return houseList;
		}
	}
}
