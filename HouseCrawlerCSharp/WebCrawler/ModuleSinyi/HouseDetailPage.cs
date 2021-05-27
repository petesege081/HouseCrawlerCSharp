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
	class HouseDetailPage : BaseHouseDetailPageModule
	{
		protected override string GetHouseDetailLink(string houseId)
		{
			return $"https://www.sinyi.com.tw/buy/house/{houseId}/?breadcrumb=list";
		}

		protected override void WaitForPageLoaded()
		{
			//Waiter.Until(cond => Js.ExecuteScript("return document.readyState").Equals("complete"));
		}

		protected override void AfterPageLoadedEvent()
		{
			//檢查是否出現An unexpected error has occurred.
			if(Driver.FindElement(By.CssSelector("body")).Text == "An unexpected error has occurred.")
			{
				throw new Exception("An unexpected error has occurred.")
				{
					Source = "WebDriver"
				};
			}

			//Click google map
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".buy-carousel-frame > .carousel-thumbnail-frame > .carousel-thumbnail-map")));
			Driver.FindElement(By.CssSelector(".buy-carousel-frame > .carousel-thumbnail-frame > .carousel-thumbnail-map")).Click();
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".buy-carousel-frame > .buy-carousel-content-frame > .static-map-img")));
			Driver.FindElement(By.CssSelector(".buy-carousel-frame > .buy-carousel-content-frame > .static-map-img")).Click();

			//Wait for google map init
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".buy-carousel-content-frame a[href*='maps.google.com/maps']")));
		}

		protected override bool CheckHouseExist()
		{
			return !Driver.Url.Contains(@"www.sinyi.com.tw/404");
		}

		public override HouseInfo GetHouseInfo(Dictionary<string, object> extras = null)
		{
			if (!IsHouseExist) return null;

			HouseInfo info = new HouseInfo
			{
				Id = HouseId,
				HouseLink = HouseLink,
				PhotoLinks = new List<string>()
			};

			Match match;
			var numberPattern = @"-?(\d+,)*\d+(.\d+)?";
			string innerHtml;

			var titleItems = Driver.FindElements(By.CssSelector(".buy-content-top-bar-title [class*='buy-content-title-']"));
			foreach(var item in titleItems)
			{
				switch(item.GetAttribute("class"))
				{
					//標題
					case "buy-content-title-name":
						info.Title = item.Text.RemoveLineBreak(true);
						break;
					//總價
					case "buy-content-title-total-price":
						match = Regex.Match(item.Text.RemoveLineBreak(true), numberPattern);
						if (match.Success)
						{
							info.TotalPrice = double.Parse(match.Groups[0].Value);
						}
						break;
					//單價
					case "buy-content-title-uni-price":
						match = Regex.Match(item.Text.RemoveLineBreak(true), numberPattern);
						if (match.Success)
						{
							info.UnitPrice = double.Parse(match.Groups[0].Value);
						}
						break;
					//社區
					case "buy-content-title-community-btn":
						info.Community = item.Text.RemoveLineBreak(true);
						break;
					//地址
					case "buy-content-title-address":
						innerHtml = item.GetAttribute("innerHTML");
						info.Address = innerHtml.RemoveTag(true);
						break;
					//含車位
					case "buy-content-title-parking":
						info.IncludeParking = true;
						break;
				}
			}

			//經緯度
			var latLng = Driver.FindElement(By.CssSelector(".buy-carousel-content-frame a[href*='maps.google.com/maps']")).GetAttribute("href").ToLatLng();
			if (latLng != null)
			{
				info.Lat = latLng.Latitude;
				info.Lng = latLng.Longitude;
			}

			var basicItems = Driver.FindElements(By.CssSelector(".buy-content-basic .buy-content-body.d-lg-block .buy-content-basic-cell, .buy-content-basic .buy-content-body.d-lg-block .buy-content-obj-cell-full-width"));
			foreach(var item in basicItems)
			{
				var field = item.FindElement(By.CssSelector("div:first-child")).Text.RemoveLineBreak(true);
				var value = item.FindElement(By.CssSelector("div:last-child")).Text.RemoveLineBreak(true);

				if (value.Contains("--"))
				{
					continue;
				}

				switch (field)
				{
					case "格局":
						var pattern = value.ToHousePattern();
						info.Bedroom = pattern.Bedroom + pattern.Room;
						info.Hall = pattern.Hall;
						info.Bathroom = pattern.Bathroom;
						info.Balcony = pattern.Balcony;
						break;
					case "屋齡":
						match = Regex.Match(value, numberPattern);
						info.Age = double.Parse(match.Groups[0].Value);
						break;
					case "樓層":
						var floorArr = value.Split("/");

						//最高樓層
						match = Regex.Match(floorArr[1], numberPattern);
						if (match.Success)
						{
							var maxFloor = int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);

							if (maxFloor == 0)
							{
								info.MaxFloor = null;
							}
							else
							{
								info.MaxFloor = maxFloor;
							}
						}

						//樓層
						match = Regex.Match(floorArr[0], @"B?-?\d+樓?(?=\D*?-\D*?\d?)");
						if (match.Success)
						{
							if (match.Groups[0].Value.Contains("B"))
							{
								info.FloorFrom = -1 * int.Parse(match.Groups[0].Value.Replace("B", ""), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
							else if (match.Groups[0].Value.Contains("樓"))
							{
								info.FloorFrom = int.Parse(match.Groups[0].Value.Replace("樓", ""), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
							else
							{
								info.FloorFrom = int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
						}

						match = Regex.Match(floorArr[0], @"(?<=\d+.*?-\D*?)B?\d+樓?");
						var floorToStr = match.Success ? match.Groups[0].Value : floorArr[0];
						match = Regex.Match(floorToStr, numberPattern);
						if (match.Success)
						{
							if (floorToStr.Contains("B"))
							{
								info.FloorTo = -1 * int.Parse(match.Groups[0].Value.Replace("B", ""), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
							else if (floorToStr.Contains("樓"))
							{
								info.FloorTo = int.Parse(match.Groups[0].Value.Replace("樓", ""), NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
							else
							{
								info.FloorTo = int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);
							}
						}
						break;
					case "大門朝向":
						info.Orientation = value;
						break;
					case "類型":
						info.BuildingType = value;
						break;
					case "車位":
						info.IncludeParking = true;
						info.ParkingType = value.Split("(")[0].Split("、")[0].Split("，")[0];
						break;
					case "建坪":
						match = Regex.Match(value, numberPattern);
						if (match.Success)
						{
							info.PingOfBuilding = double.Parse(match.Groups[0].Value);
						}
						break;
					case "地坪":
						match = Regex.Match(value, numberPattern);
						if (match.Success)
						{
							info.PingOfLand = double.Parse(match.Groups[0].Value);
						}
						break;
				}
			}

			//坪數細節
			var pingItems = Driver.FindElements(By.CssSelector(".buy-content-basic .buy-content-basic-active-area .buy-content-basic-cell, .buy-content-basic .buy-content-basic-active-area .buy-content-obj-cell-full-width"));
			foreach(var item in pingItems)
			{
				var field = item.FindElement(By.CssSelector("div:first-child")).Text.RemoveLineBreak(true);
				var value = item.FindElement(By.CssSelector("div:last-child")).Text.RemoveLineBreak(true);

				if (value.Contains("--"))
				{
					continue;
				}

				match = Regex.Match(value, numberPattern);
				if (match.Success)
				{
					var pingValue = double.Parse(match.Groups[0].Value);
					
					if(field.Contains("停車"))
					{
						info.PingOfParking = pingValue;
						continue;
					}

					switch(field){
						case "主+陽":
							continue;
						case "主建物":
							info.PingOfIndoor = pingValue;
							break;
						case "公共設施":
							info.PingOfShared = pingValue;
							break;
						case "陽台":
							info.PingOfBalcony = pingValue;
							if(pingValue > 0)
							{
								info.Balcony = 1;
							}
							info.PingOfAncillary += pingValue;
							break;
						default:
							info.PingOfAncillary += pingValue;
							break;
					}
				}
			}

			var objectItems = Driver.FindElements(By.CssSelector(".buy-content-obj-detail.d-lg-block .buy-content-obj-cell, .buy-content-obj-detail.d-lg-block .buy-content-obj-cell-full-width"));
			foreach (var item in objectItems)
			{
				var field = item.FindElement(By.CssSelector("div:first-child")).Text.RemoveLineBreak(true);
				var value = item.FindElement(By.CssSelector("div:last-child")).Text.RemoveLineBreak(true);

				if (value.Contains("--"))
				{
					continue;
				}

				switch (field)
				{
					case "謄本用途":
						info.BuildingUsage = value;
						break;
				}
			}

			//格局圖片
			var patternFrame = Driver.FindElements(By.CssSelector(".buy-carousel-frame > .carousel-thumbnail-frame > div"));
			foreach (var item in patternFrame)
			{
				switch (item.GetAttribute("class"))
				{
					case "carousel-thumbnail-pattern":
					//case "carousel-thumbnail-map":
						match = Regex.Match(item.GetAttribute("style"), @"(?<=background-image:\s*url\(\s*"").*?(?=""\s*\))");
						info.PhotoLinks.Add(match.Groups[0].Value);
						break;
				}
			}

			//房屋照片
			var thumbnailFrame = Driver.FindElements(By.CssSelector(".buy-carousel-frame .carousel-thumbnail-img-frame div[src]"));
			foreach (var item in thumbnailFrame)
			{
				info.PhotoLinks.Add(item.GetAttribute("src"));
			}

			//處理圖片網址異常
			for (var i = info.PhotoLinks.Count - 1; i >= 0; i--)
			{
				//Case 1. No image url
				if (info.PhotoLinks[i].EndsWith("noimge.jpg", StringComparison.OrdinalIgnoreCase)
					|| info.PhotoLinks[i].EndsWith("media.cthouse.com.tw/photo/", StringComparison.OrdinalIgnoreCase))
				{
					info.PhotoLinks.RemoveAt(i);
					continue;
				}

				//Case 2. ex:https://media.cthouse.com.tw/photo//project2////house_photo/202105//1652514-4_new.jpg
				info.PhotoLinks[i] = Regex.Replace(info.PhotoLinks[i], @"(?<=https?:/.*?/)/(?=.+?)", "");
			}

			return info;
		}
	}
}
