using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.WebCrawler.YungChing
{
	class HouseListPage : BaseHouseListPageModule
	{
		protected override string GetHouseListLink(string regionKey, int? orderBy)
		{
			var orderStr = orderBy switch
			{
				HouseOrderBy.PUBLISH_DESC => "80",
				//HouseOrderBy.PUBLISH_ASC => "publish-asc",
				_ => "",
			};

			var url = $"https://buy.yungching.com.tw/region/{regionKey}/";
			if (!string.IsNullOrWhiteSpace(orderStr))
			{
				url += $"?od={orderStr}";
			}

			return url;
		}

		protected override void WaitForPageLoaded()
		{
			Waiter.Until(cond => Js.ExecuteScript("return document.readyState").Equals("complete"));
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
			PageIndex = int.Parse(Driver.FindElement(By.CssSelector(".m-pagination-bd > .is-active")).Text);

			//Check current page is last page
			try
			{
				var endBtn = Driver.FindElement(By.CssSelector(".m-pagination-bd > :last-child"));
				HasNextPage = endBtn.GetAttribute("class") != "disabled";
			}
			catch (NoSuchElementException)
			{
				HasNextPage = false;
			}
		}

		public override BaseHouseListPageModule NextPage()
		{
			Driver.FindElement(By.CssSelector(".m-pagination-bd > :nth-last-child(2)")).Click();

			//Wait for content load
			WaitForPageLoaded();
			AfterPageLoadedEvent();

			return this;
		}

		protected override int GetHouseCount()
		{
			var totalStr = Driver.FindElement(By.CssSelector(".list-hd-inner > [ga-label='buy_filter_tag_none']")).Text;
			var match = Regex.Match(totalStr, @"(\d+,)*\d+");
			return match.Success ? int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands) : 0;
		}

		public override List<HouseListItem> GetHouseList()
		{
			var houseIds = new List<HouseListItem>();

			var cards = Driver.FindElements(By.CssSelector(".l-main-list > .l-item-list .item-info"));

			Match match;
			foreach (var card in cards)
			{
				var listItem = new HouseListItem
				{
					Extras = new Dictionary<string, object>()
				};

				//HouseId
				match = Regex.Match(card.FindElement(By.CssSelector(".item-title")).GetAttribute("href"), @"(?<=/house/).+$");
				if(match.Success){
					listItem.HouseId = match.Groups[0].Value.Trim();
					
				}

				//永慶的Detail頁面較難結構化, 先在List頁面先取得部分資料, 再帶到Detail頁面處理
				listItem.Extras.Add("1", card.FindElement(By.CssSelector(".item-info-detail > :nth-child(1)")).Text.RemoveLineBreak(true));
				listItem.Extras.Add("2", card.FindElement(By.CssSelector(".item-info-detail > :nth-child(2)")).Text.RemoveLineBreak(true));
				listItem.Extras.Add("3", card.FindElement(By.CssSelector(".item-info-detail > :nth-child(3)")).Text.RemoveLineBreak(true));
				listItem.Extras.Add("7", card.FindElement(By.CssSelector(".item-info-detail > :nth-child(7)")).Text.RemoveLineBreak(true));

				var tagsItems = card.FindElements(By.CssSelector(".item-tags > span"));
				var tags = new List<string>();
				foreach(var item in tagsItems)
				{
					tags.Add(item.Text.RemoveLineBreak(true));
				}
				listItem.Extras.Add("T", tags);

				houseIds.Add(listItem);
			}

			return houseIds;
		}
	}
}
