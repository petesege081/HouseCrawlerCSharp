using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.WebCrawler.YungChing
{
	class HouseDetailPage : BaseHouseDetailPageModule
	{
		protected override string GetHouseDetailLink(string houseId)
		{
			return $"https://buy.yungching.com.tw/house/{houseId}";
		}

		protected override void WaitForPageLoaded()
		{
			//Waiter.Until(cond => Js.ExecuteScript("return document.readyState").Equals("complete"));
		}

		protected override void AfterPageLoadedEvent()
		{
			//Click google map
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".m-house-photos-handlers > .house-info-img.carousel-map > a")));
			Js.ExecuteScript("window.scrollTo(0, 250)"); 
			Driver.FindElement(By.CssSelector(".m-house-photos-handlers > .house-info-img.carousel-map > a")).Click();
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".house-photos-main-map-wrap.is-active > .house-photos-main-map")));
			Driver.FindElement(By.CssSelector(".house-photos-main-map-wrap.is-active > .house-photos-main-map")).Click();

			//Wait for google map init
			Waiter.Until(ExpectedConditions.ElementExists(By.CssSelector(".house-photos-main-map-wrap.is-active a[href*='maps.google.com/maps']")));
		}

		protected override bool CheckHouseExist()
		{
			return !Driver.Url.Contains(@"buy.yungching.com.tw/Information/CaseNotFound");
		}

		public override HouseInfo GetHouseInfo(Dictionary<string, object> extras = null)
		{
			if (!IsHouseExist) return null;

			Watcher.Restart();

			HouseInfo info = new HouseInfo
			{
				Id = HouseId,
				HouseLink = HouseLink,
				PhotoLinks = new List<string>()
			};

			Match match;
			var numberPattern = @"-?(\d+,)*\d+(.\d+)?";
			var tags = new List<string>();

			//處理從List頁面取得的資料
			if(extras != null)
			{
				//將Tags轉回原型
				foreach(var tag in ((IList)extras["T"]).OfType<object>().ToList())
				{
					tags.Add(tag.ToString());
				}

				foreach (var kv in extras)
				{
					var value = kv.Value as string;

					if(string.IsNullOrWhiteSpace(value)){
						continue;
					}

					switch (kv.Key)
					{
						//建物類型
						case "1":
							info.BuildingType = value;
							break;
						//屋齡
						case "2":
							match = Regex.Match(value, numberPattern);
							if(match.Success)
							{
								info.Age = double.Parse(match.Groups[0].Value);
							}
							break;
						//樓層
						case "3":
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
								else if (floorArr[1].Contains("B"))
								{
									info.MaxFloor = -1 * maxFloor;
								}
								else
								{
									info.MaxFloor = maxFloor;
								}
							}

							//非單一樓層時, 取最高樓層
							var fromToArr = floorArr[0].Split("~");
							match = Regex.Match(fromToArr[0], numberPattern);
							if (match.Success)
							{
								var floorFrom = int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);

								if (floorFrom == 0)
								{
									info.FloorFrom = null;
								}
								else if (fromToArr[0].Contains("B"))
								{
									info.FloorFrom = -1 * floorFrom;
								}
								else
								{
									info.FloorFrom = floorFrom;
								}
							}

							match = Regex.Match(fromToArr[1], numberPattern);
							if (match.Success)
							{
								var floorTo = int.Parse(match.Groups[0].Value, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign);

								if (floorTo == 0)
								{
									info.FloorTo = null;
								}
								else if (fromToArr[1].Contains("B"))
								{
									info.FloorTo = -1 * floorTo;
								}
								else
								{
									info.FloorTo = floorTo;
								}
							}

							break;
						//格局
						case "7":
							var pattern = value.ToHousePattern();
							info.Bedroom = pattern.Bedroom + pattern.Room;
							info.Hall = pattern.Hall;
							info.Bathroom = pattern.Bathroom;
							info.Balcony = pattern.Balcony;
							break;
					}
				}
			}

			//標題
			info.Title = Driver.FindElement(By.CssSelector(".m-info-house .house-info-name")).Text.RemoveLineBreak(true);
			//總價
			match = Regex.Match(Driver.FindElement(By.CssSelector(".m-info-house .price-num")).Text, numberPattern);
			if (match.Success)
			{
				info.TotalPrice = double.Parse(match.Groups[0].Value);
			}
			//地址
			info.Address = Driver.FindElement(By.CssSelector(".m-info-house .house-info-addr")).Text.RemoveLineBreak(true);

			//經緯度
			var latLng = Driver.FindElement(By.CssSelector(".house-photos-main-map a[href*='maps.google.com/maps']")).GetAttribute("href").ToLatLng();
			if (latLng != null)
			{
				info.Lat = latLng.Latitude;
				info.Lng = latLng.Longitude;
			}

			//House detail box
			var sections = Driver.FindElements(By.CssSelector(".m-house-detail-block.detail-data > section"));
			foreach (var section in sections)
			{
				var cssClass = section.GetAttribute("class").Split(" ").FirstOrDefault(c => c.StartsWith("bg-"));

				switch (cssClass)
				{
					//單價
					case "bg-price":
						var priceStr = section.FindElement(By.CssSelector(".right > span")).Text.RemoveLineBreak().Replace(" ", "");
						match = Regex.Match(priceStr, @$"(?<=單價.*?){numberPattern}(?=.*?萬)");
						if(match.Success)
						{
							info.UnitPrice = double.Parse(match.Groups[0].Value);
						}
						break;
					//停車位
					case "bg-car":
						info.IncludeParking = true;
						info.ParkingType = section.FindElement(By.CssSelector("li")).Text.RemoveLineBreak().Replace("固定車位", "").Trim();
						break;
					//其他資訊
					case "bg-other":
						//社區(如果有社區資料, 會出現在第一個)
						var firstItem = section.FindElement(By.XPath("./div[1]/ul/li[1]"));
						if (firstItem.Text.Contains("追蹤社區新案"))
						{
							info.Community = firstItem.FindElement(By.XPath("./a[1]")).Text.RemoveLineBreak(true);
						}
						else
						{
							//沒關鍵字時比對List頁面中的Tags, 有相同的項目則為社區
							var value = firstItem.Text.RemoveLineBreak(true);
							if(tags.Exists(o => o == value)){
								info.Community = value;
							}
						}

						//朝向
						var rightSideStr = section.FindElement(By.XPath("./div[2]")).Text.RemoveLineBreak();
						match = Regex.Match(rightSideStr, @"(?<=朝向)\S+");
						if(match.Success)
						{
							info.Orientation = match.Groups[0].Value;
						}
						break;
					//坪數細節
					case "bg-square":
						var text = section.Text;

						match = Regex.Match(text, @$"(?<=土地坪數：\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfLand = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=登記用途：\s*?)\S+");
						if (match.Success)
						{
							info.BuildingUsage = match.Groups[0].Value;
						}

						match = Regex.Match(text, @$"(?<=建物坪數：\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfBuilding = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=含車位\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfParking = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=主建物小計：\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfIndoor = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=共同使用小計：\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfShared = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=附屬建物小計：\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.PingOfAncillary = double.Parse(match.Groups[0].Value);
						}

						match = Regex.Match(text, @$"(?<=陽台\s*?){numberPattern}(?=\s*?坪)");
						if (match.Success)
						{
							info.Balcony = 1;
							info.PingOfBalcony = double.Parse(match.Groups[0].Value);
						}
						break;
				}
			}

			//永慶通常不會提供單價, 以程式來計算
			if(info.UnitPrice == 0d){
				info.UnitPrice = Math.Round(info.TotalPrice / info.PingOfBuilding, 2, MidpointRounding.AwayFromZero);
			}

			//格局圖片&房屋照片
			var photoHandlers = Driver.FindElements(By.CssSelector(".m-house-photos-handlers > div"));
			foreach (var handler in photoHandlers)
			{
				var cssClass = handler.GetAttribute("class");
				if (cssClass.Contains("layout-img"))
				{
					var url = handler.FindElement(By.CssSelector("img")).GetAttribute("src");

					if(url.EndsWith("non-layout.png"))
					{
						continue;
					}

					//換成大圖(1024x786)
					url = Regex.Replace(url, @"(?<=width=)\d+", "1024");
					url = Regex.Replace(url, @"(?<=height=)\d+", "786");

					info.PhotoLinks.Add(url);
				}
				else if (cssClass.Contains("house-info-img-list"))
				{
					var imgs = handler.FindElements(By.CssSelector(".m-house-photos-list img"));
					foreach (var img in imgs)
					{
						var url = img.GetAttribute("src");

						//換成大圖(1024x786)
						url = Regex.Replace(url, @"(?<=width=)\d+", "1024");
						url = Regex.Replace(url, @"(?<=height=)\d+", "786");

						info.PhotoLinks.Add(url);
					}
				}
			}

			Watcher.Stop();
			Timer.DataCapture = Watcher.ElapsedMilliseconds;

			return info;
		}
	}
}
