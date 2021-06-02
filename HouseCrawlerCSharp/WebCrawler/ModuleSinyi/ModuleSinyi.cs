using HouseCrawlerCSharp.Model;
using HouseCrawlerCSharp.WebCrawler.Sinyi;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.WebCrawler
{
	class ModuleSinyi : BaseCrawlerModule
	{
		public override BaseHouseListPageModule CreateHouseListPage()
		{
			return new HouseListPage();
		}

		public override BaseHouseDetailPageModule CreateHouseDetailPage()
		{
			return new HouseDetailPage();
		}

		protected override string GetModuleType()
		{
			return "Sinyi";
		}

		protected override CrawlerProcessData GetDefaultProcessData()
		{
			return new CrawlerProcessData
			{
				Regions = new List<RegionData>
				{
					new RegionData { Code = "TPE", Name = "Taipei City",       SiteKey = "Taipei-city",        Status = RegionStatus.QUEUE },
					new RegionData { Code = "KLU", Name = "Keelung City",      SiteKey = "Keelung-city",       Status = RegionStatus.QUEUE },
					new RegionData { Code = "TPH", Name = "New Taipei City",   SiteKey = "NewTaipei-city",     Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSC", Name = "Hsinchu City",      SiteKey = "Hsinchu-city",       Status = RegionStatus.QUEUE },
					new RegionData { Code = "HSH", Name = "Hsinchu County",    SiteKey = "Hsinchu-county",     Status = RegionStatus.QUEUE },
					new RegionData { Code = "TYC", Name = "Taoyuan City",      SiteKey = "Taoyuan-city",       Status = RegionStatus.QUEUE },
					new RegionData { Code = "MAL", Name = "Miaoli County",     SiteKey = "Miaoli-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "TXG", Name = "Taichung City",     SiteKey = "Taichung-city",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "CWH", Name = "Changhua County",   SiteKey = "Changhua-county",    Status = RegionStatus.QUEUE },
					new RegionData { Code = "NTO", Name = "Nantou County",     SiteKey = "Nantou-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "CYI", Name = "Chiayi City",       SiteKey = "Chiayi-city",        Status = RegionStatus.QUEUE },
					new RegionData { Code = "CHY", Name = "Chiayi County",     SiteKey = "Chiayi-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "YLH", Name = "Yunlin County",     SiteKey = "Yunlin-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "TNN", Name = "Tainan City",       SiteKey = "Tainan-city",        Status = RegionStatus.QUEUE },
					new RegionData { Code = "KHH", Name = "Kaohsiung City",    SiteKey = "Kaohsiung-city",     Status = RegionStatus.QUEUE },
					new RegionData { Code = "IUH", Name = "Pingtung County",   SiteKey = "Pingtung-county",    Status = RegionStatus.QUEUE },
					new RegionData { Code = "ILN", Name = "Yilan County",      SiteKey = "Yilan-county",       Status = RegionStatus.QUEUE },
					new RegionData { Code = "TTT", Name = "Taitung County",    SiteKey = "Taitung-county",     Status = RegionStatus.QUEUE },
					new RegionData { Code = "HWA", Name = "Hualian County",    SiteKey = "Hualien-county",     Status = RegionStatus.QUEUE },
					new RegionData { Code = "PEH", Name = "Penghu County",     SiteKey = "Penghu-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "KMN", Name = "Kinmen County",     SiteKey = "Kinmen-county",      Status = RegionStatus.QUEUE },
					new RegionData { Code = "LNN", Name = "Lienchiang County", SiteKey = "Lienchiang-county", Status = RegionStatus.QUEUE }
				}
			};
		}
	}
}
