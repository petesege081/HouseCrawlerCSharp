using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler.YungChing;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.WebCrawler
{
	class ModuleYungChing : BaseCrawlerModule
	{
		protected override BaseHouseListPageModule InitHouseListPage()
		{
			return HouseListPage = new HouseListPage();
		}

		protected override BaseHouseDetailPageModule InitHouseDetailPage()
		{
			return HouseDetailPage = new HouseDetailPage();
		}

		protected override string GetModuleType()
		{
			return "YungChing";
		}

		protected override CrawlerProcessData GetDefaultProcessData()
		{
			return new CrawlerProcessData
			{
				Regions = new List<RegionData>
				{
					new RegionData { Code = "TPE", Name = "Taipei City",       SiteKey = "台北市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "KLU", Name = "Keelung City",      SiteKey = "基隆市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TPH", Name = "New Taipei City",   SiteKey = "新北市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSC", Name = "Hsinchu City",      SiteKey = "新竹市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSH", Name = "Hsinchu County",    SiteKey = "新竹縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TYC", Name = "Taoyuan City",      SiteKey = "桃園市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "MAL", Name = "Miaoli County",     SiteKey = "苗栗縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TXG", Name = "Taichung City",     SiteKey = "台中市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "CWH", Name = "Changhua County",   SiteKey = "彰化縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "NTO", Name = "Nantou County",     SiteKey = "南投縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "CYI", Name = "Chiayi City",       SiteKey = "嘉義市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "CHY", Name = "Chiayi County",     SiteKey = "嘉義縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "YLH", Name = "Yunlin County",     SiteKey = "雲林縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TNN", Name = "Tainan City",       SiteKey = "台南市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "KHH", Name = "Kaohsiung City",    SiteKey = "高雄市-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "IUH", Name = "Pingtung County",   SiteKey = "屏東縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "ILN", Name = "Yilan County",      SiteKey = "宜蘭縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TTT", Name = "Taitung County",    SiteKey = "台東縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "HWA", Name = "Hualian County",    SiteKey = "花蓮縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "PEH", Name = "Penghu County",     SiteKey = "澎湖縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "KMN", Name = "Kinmen County",     SiteKey = "金門縣-_c", Status = RegionStatus.QUEUE },
					new RegionData { Code = "LNN", Name = "Lienchiang County", SiteKey = "連江縣-_c", Status = RegionStatus.SKIP  } //永慶沒有連江縣
				}
			};
		}
	}
}
