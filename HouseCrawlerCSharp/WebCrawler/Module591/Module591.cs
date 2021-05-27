using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler._591;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.WebCrawler
{
	class Module591 : BaseCrawlerModule
	{
		public override BaseHouseListPageModule CreateHouseListPage()
		{
			return new HouseListPage();
		}

		public override BaseHouseDetailPageModule CreateHouseDetailPage()
		{
			return  new HouseDetailPage();
		}

		protected override string GetModuleType()
		{
			return "591";
		}

		protected override CrawlerProcessData GetDefaultProcessData()
		{
			return new CrawlerProcessData
			{
				Regions = new List<RegionData>
				{
					new RegionData { Code = "TPE", Name = "Taipei City",       SiteKey = "1",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "KLU", Name = "Keelung City",      SiteKey = "2",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "TPH", Name = "New Taipei City",   SiteKey = "3",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSC", Name = "Hsinchu City",      SiteKey = "4",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSH", Name = "Hsinchu County",    SiteKey = "5",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "TYC", Name = "Taoyuan City",      SiteKey = "6",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "MAL", Name = "Miaoli County",     SiteKey = "7",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "TXG", Name = "Taichung City",     SiteKey = "8",  Status = RegionStatus.QUEUE },
					new RegionData { Code = "CWH", Name = "Changhua County",   SiteKey = "10", Status = RegionStatus.QUEUE },
					new RegionData { Code = "NTO", Name = "Nantou County",     SiteKey = "11", Status = RegionStatus.QUEUE },
					new RegionData { Code = "CYI", Name = "Chiayi City",       SiteKey = "12", Status = RegionStatus.QUEUE },
					new RegionData { Code = "CHY", Name = "Chiayi County",     SiteKey = "13", Status = RegionStatus.QUEUE },
					new RegionData { Code = "YLH", Name = "Yunlin County",     SiteKey = "14", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TNN", Name = "Tainan City",       SiteKey = "15", Status = RegionStatus.QUEUE },
					new RegionData { Code = "KHH", Name = "Kaohsiung City",    SiteKey = "17", Status = RegionStatus.QUEUE },
					new RegionData { Code = "IUH", Name = "Pingtung County",   SiteKey = "19", Status = RegionStatus.QUEUE },
					new RegionData { Code = "ILN", Name = "Yilan County",      SiteKey = "21", Status = RegionStatus.QUEUE },
					new RegionData { Code = "TTT", Name = "Taitung County",    SiteKey = "22", Status = RegionStatus.QUEUE },
					new RegionData { Code = "HWA", Name = "Hualian County",    SiteKey = "23", Status = RegionStatus.QUEUE },
					new RegionData { Code = "PEH", Name = "Penghu County",     SiteKey = "24", Status = RegionStatus.QUEUE },
					new RegionData { Code = "KMN", Name = "Jinmen County",     SiteKey = "25", Status = RegionStatus.QUEUE },
					new RegionData { Code = "LNN", Name = "Lienchiang County", SiteKey = "26", Status = RegionStatus.QUEUE }
				}
			};
		}

		
	}
}
