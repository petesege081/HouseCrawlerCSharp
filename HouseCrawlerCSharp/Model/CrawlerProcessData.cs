using System.Collections.Generic;

namespace HouseCrawlerCSharp.Model
{
	class CrawlerProcessData
	{
		public List<RegionData> Regions;
		public List<HouseListItem> HouseList = new List<HouseListItem>();
		public List<FailedCase> FailedCases = new List<FailedCase>();
		public int Cursor = 0;
		public int TotalRows = 0;
	}

	class RegionData{
		public int Status;
		public string Name;
		public string Code;
		public string SiteKey;
	}

	static class RegionStatus
	{
		public const int QUEUE = 1;
		public const int IN_LIST_PROCESS = 2;
		public const int IN_DETAIL_PROCESS = 3;
		public const int DONE = 4;
		public const int SKIP = 5;
	}

	static class HouseOrderBy
	{
		public const int DEFAULT = 1;
		public const int PUBLISH_ASC = 2;
		public const int PUBLISH_DESC = 3;
		public const int TOTAL_PRICE_ASC = 4;
		public const int TOTAL_PRICE_DESC = 5;
		public const int UNIT_PRICE_ASC = 6;
		public const int UNIT_PRICE_DESC = 7;
		public const int PING_ASC = 8;
		public const int PING_DESC = 9;
		public const int AGE_ASC = 10;
		public const int AGE_DESC = 11;
		public const int DISCOUNT_ASC = 12;
		public const int DISCOUNT_DESC = 13;
	}

	class FailedCase{
		public HouseListItem HouseItem;
		public int RetryCount = 0;
	}
}
