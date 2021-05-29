using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.WebCrawler.Sinyi
{
	class HouseListPage : BaseHouseListPageModule
	{
		protected override string GetHouseListLink(string regionKey, int page, string extraParam, int? orderBy)
		{
			var orderStr = orderBy switch
			{
				HouseOrderBy.PUBLISH_DESC => "publish-desc",
				//HouseOrderBy.PUBLISH_ASC => "publish-asc",
				_ => "",
			};

			var url = $"https://www.sinyi.com.tw/buy/list/{regionKey}/";
			if (!string.IsNullOrWhiteSpace(orderStr))
			{
				url += $"{orderStr}/";
			}

			url += page == 1 ? "index" : $"{page}";

			return url;
		}

		protected override void WaitForPageLoaded()
		{
			//等待Loading圖片消失
			Waiter.Until(cond =>
			{
				try
				{
					return !Driver.FindElement(By.CssSelector(".buy-list-frame > :first-child")).GetAttribute("class").Contains("loading-frame-lc");
				}
				catch (Exception)
				{
					return false;
				}
			});
		}

		protected override void AfterPageLoadedEvent()
		{
			//處理該縣市底下無資料的情況
			if(PageIndex == 0 && GetHouseCount() == 0)
			{
				PageIndex = 1;
				HasNextPage = false;
				return;
			}

			//Change current page index
			PageIndex = int.Parse(Driver.FindElement(By.CssSelector(".pagination > .activeClassName > .pageLinkClassName")).Text);

			//Check current page is last page
			try
			{
				var currBtn = Driver.FindElement(By.CssSelector(".pagination > :nth-last-child(2)"));
				HasNextPage = !currBtn.GetAttribute("class").Contains("activeClassName");
			}
			catch (NoSuchElementException)
			{
				HasNextPage = false;
			}
		}

		public override BaseHouseListPageModule NextPage()
		{
			Driver.FindElement(By.CssSelector(".pagination > .nextClassName > .nextLinkClassName")).Click();

			//Wait for content load
			WaitForPageLoaded();
			AfterPageLoadedEvent();

			return this;
		}

		public override int GetHouseCount()
		{
			if(HouseCount < 0){
				var listFrame = Driver.FindElement(By.CssSelector(".buy-list-frame"));
				var parent = listFrame.FindElement(By.XPath("./.."));
				var totalStr = parent.FindElement(By.CssSelector(":first-child .d-lg-none")).GetAttribute("innerHTML").RemoveTag(true);

				var match = Regex.Match(totalStr, @"(\d+,)*\d+");
				HouseCount = match.Success ? int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands) : 0;
			}

			return HouseCount;
		}

		public override List<HouseListItem> GetHouseList()
		{
			Watcher.Restart();

			List<HouseListItem> houseList = new List<HouseListItem>();

			var cards = Driver.FindElements(By.CssSelector(".buy-list-frame > .buy-list-item > a"));

			Match match;
			foreach (var card in cards)
			{
				match = Regex.Match(card.GetAttribute("href"), @"(?<=/buy/house/).*?(?=/)");

				if(match.Success)
				{
					houseList.Add(new HouseListItem { HouseId = match.Groups[0].Value });
				}
			}

			Watcher.Stop();
			Timer.DataCapture = Watcher.ElapsedMilliseconds;

			return houseList;
		}
	}
}
