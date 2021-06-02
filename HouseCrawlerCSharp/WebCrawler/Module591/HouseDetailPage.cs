using HouseCrawlerCSharp.Library;
using HouseCrawlerCSharp.Model;
using NLog;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.WebCrawler._591
{
	class HouseDetailPage : BaseHouseDetailPageModule
	{
		protected override string GetHouseDetailLink(string houseId)
		{
			return $"https://sale.591.com.tw/home/house/detail/2/{houseId}.html#detail-map";
		}

		protected override void WaitForPageLoaded()
		{
			//Scroll to bottom, and wait for google map display.
			Waiter.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("detail-map-free")));
		}

		protected override void AfterPageLoadedEvent()
		{

		}

		protected override bool CheckHouseExist()
		{
			try
			{
				Driver.FindElement(By.CssSelector(".detail-title-container"));
				return true;
			}
			catch (NoSuchElementException)
			{
				
			}
			return false;
		}
		
		public override HouseInfo GetHouseInfo(Dictionary<string, object> extras = null)
		{
			if (!IsHouseExist) return null;

			Watcher.Restart();

			HouseInfo info = new HouseInfo
			{
				Id = HouseId,
				HouseLink = HouseLink.Split("#")[0],
				PhotoLinks = new List<string>()
			};

			Match match;
			string innerHtml;

			//標題
			info.Title = Driver.FindElement(By.CssSelector(".detail-title-content")).GetAttribute("innerHTML").RemoveTag();
			//總價
			innerHtml = Driver.FindElement(By.CssSelector(".info-price-num")).GetAttribute("innerHTML");
			try
			{
				info.TotalPrice = double.Parse(innerHtml.RemoveTag(true));
			}
			catch (FormatException ex)
			{
				throw new FormatException($"{ex.Message}\nInput : {innerHtml}", ex);
			}
			//單價
			innerHtml = Driver.FindElement(By.CssSelector(".info-price-per")).GetAttribute("innerHTML");
			match = Regex.Match(innerHtml.RemoveTag(true), @"(-?\d+(\.\d+)?)萬/坪");
			if(match.Success)
			{
				info.UnitPrice = double.Parse(match.Groups[0].Value.Replace("萬/坪", ""));
			}

			//經緯度
			var latLng = Driver.FindElement(By.CssSelector(".detail-map-box .datalazyload")).GetAttribute("value").ToLatLng();
			if(latLng != null)
			{
				info.Lat = latLng.Latitude;
				info.Lng = latLng.Longitude;
			}

			var floorBox = Driver.FindElements(By.CssSelector(".info-box-floor > .info-floor-left"));
			foreach (var item in floorBox)
			{
				var key = item.FindElement(By.CssSelector(".info-floor-value")).Text.RemoveLineBreak(true);
				innerHtml = item.FindElement(By.CssSelector(".info-floor-key")).GetAttribute("innerHTML");

				try
				{
					switch (key)
					{
						case "格局":
							var pattern = innerHtml.RemoveTag(true).ToHousePattern();
							info.Bedroom = pattern.Bedroom;
							info.Hall = pattern.Hall;
							info.Bathroom = pattern.Bathroom;
							info.Balcony = pattern.Balcony;
							break;
						case "屋齡":
							var ageStr = innerHtml.RemoveTag(true);

							match = Regex.Match(ageStr, @"\d+年");
							if (match.Success)
							{
								info.Age = double.Parse(match.Groups[0].Value.Replace("年", ""));
								break;
							}

							match = Regex.Match(ageStr, @"\d+個月");
							if (match.Success)
							{
								info.Age = Math.Round(double.Parse(match.Groups[0].Value.Replace("個月", "")) / 12, 1);
								break;
							}

							//屋齡不詳
							info.Age = -1;
							break;
						case "權狀坪數":
							var pingStr = innerHtml.RemoveTag(true);
							match = Regex.Match(pingStr, @"(-?\d+(\.\d+)?)坪");
							if (match.Success)
							{
								info.PingOfBuilding = double.Parse(match.Groups[0].Value.Replace("坪", ""));
							}

							if (pingStr.Contains("(含車位"))
							{
								info.IncludeParking = true;
							}
							break;
					}
				}
				catch (FormatException ex)
				{
					throw new FormatException($"{ex.Message}\nInput : {innerHtml}", ex);
				}
			}

			var addrBox = Driver.FindElements(By.CssSelector(".info-box-addr > .info-addr-content"));
			foreach (var item in addrBox)
			{
				var field = item.FindElement(By.CssSelector(".info-addr-key")).Text.RemoveLineBreak(true);
				var value = item.FindElement(By.CssSelector(".info-addr-value")).Text.RemoveLineBreak(true);

				if (string.IsNullOrEmpty(value))
				{
					continue;
				}

				switch (field)
				{
					case "樓層":
						var floorArr = value.Split("/");

						//最高樓層
						if (floorArr[1].Contains("F"))
						{
							info.MaxFloor = int.Parse(floorArr[1].Replace("F", ""));
						}


						if (floorArr[0].Contains("B"))
						{
							info.FloorFrom = -1 * int.Parse(floorArr[0].Replace("B", ""));
							info.FloorTo = info.FloorFrom;
						}
						else if (floorArr[0].Contains("F"))
						{
							info.FloorFrom = int.Parse(floorArr[0].Replace("F", ""));
							info.FloorTo = info.FloorFrom;
						}
						else if (floorArr[0].Contains("整棟") && info.MaxFloor != null)
						{
							info.FloorFrom = 1;
							info.FloorTo = info.MaxFloor;
						}
						break;
					case "朝向":
						info.Orientation = value.Split("朝")[1];
						break;
					case "社區":
						info.Community = value;
						break;
					case "地址":
						info.Address = item.FindElement(By.CssSelector(".info-addr-value")).GetAttribute("innerHTML").RemoveTag();
						break;
				}
			}

			var detailBox = Driver.FindElements(By.CssSelector(".detail-house-box"));
			foreach (var categoryItem in detailBox)
			{
				var category = categoryItem.FindElement(By.CssSelector(".detail-house-name")).Text.RemoveLineBreak(true);

				switch (category)
				{
					case "房屋資料":
					case "坪數說明":
						var fieldBox = categoryItem.FindElements(By.CssSelector(".detail-house-item"));

						foreach (var fieldItem in fieldBox)
						{
							var field = fieldItem.FindElement(By.CssSelector(".detail-house-key")).Text.RemoveLineBreak(true);
							var value = fieldItem.FindElement(By.CssSelector(".detail-house-value")).Text.RemoveLineBreak(true);

							if(string.IsNullOrEmpty(value))
							{
								continue;
							}

							try
							{
								switch (field)
								{
									case "型態":
										info.BuildingType = value;
										break;
									case "法定用途":
										info.BuildingUsage = value;
										break;
									case "車位":
										if (value != "無")
										{
											info.IncludeParking = true;
											var parkingArr = value.Split("，");

											foreach (var pVal in parkingArr)
											{
												if (pVal.Contains("坪"))
												{
													info.PingOfParking = double.Parse(pVal.Replace("坪", ""));
												}
												else if (pVal.Contains("式"))
												{
													info.ParkingType = pVal;
												}
											}
										}
										break;
									case "主建物":
										info.PingOfIndoor = double.Parse(value.Replace("坪", ""));
										break;
									case "共用部分":
										info.PingOfShared = double.Parse(value.Replace("坪", ""));
										break;
									case "附屬建物":
										info.PingOfAncillary = double.Parse(value.Replace("坪", ""));
										break;
									case "土地坪數":
										info.PingOfLand = double.Parse(value.Replace("坪", ""));
										break;
								}
							}
							catch (FormatException ex)
							{
								throw new FormatException($"{ex.Message}\nInput : {value}", ex);
							}
						}
						break;
				}
			}

			//圖片網址
			var imgList = Driver.FindElements(By.CssSelector(".house-pic-box .pic-box-img"));
			foreach (var img in imgList)
			{
				info.PhotoLinks.Add(img.GetAttribute("data-original"));
			}

			Watcher.Stop();
			Timer.DataCapture = Watcher.ElapsedMilliseconds;

			return info;
		}
	}
}
