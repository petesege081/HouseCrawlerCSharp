using FileHelpers;
using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace HouseCrawlerCSharp.Model
{
	class HousePattern {
		public int Bedroom; //房
		public int Bathroom; //衛
		public int Hall; //廳
		public int Balcony; //陽台
		public int Room; //室
	}

	class PingCollection {
		public double Building; //權狀坪數
		public double Indoor; //主建物坪數
		public double Shared; //共用坪數
		public double Ancillary; //附屬建物坪數
		public double Balcony; //陽台坪數
		public double Land; //土地坪數
		public double Parking; //車位
	}

	[DelimitedRecord(","), IgnoreFirst(1)]
	class HouseInfo
	{
		[FieldHidden]
		public static readonly string[] Headers = new string[] { 
			"房屋代碼", "標題", "總價", "單價", "房",
			"廳", "衛", "陽台", "屋齡", "樓層", 
			"最高樓層", "朝向", "社區", "地址", "緯度", 
			"經度", "房屋類型", "登記用途", "權狀坪數", "主建物坪數",
			"共用坪數", "附屬建物坪數", "陽台坪數", "土地坪數", "含車位", 
			"車位類型", "車位坪數", "照片目錄", "連結", "建立時間" };

		[FieldOrder(1)]
		public string Id = "";
		[FieldOrder(2)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string Title = "";
		[FieldOrder(3)]
		public double TotalPrice; //總價
		[FieldOrder(4)]
		public double UnitPrice; //單價

		[FieldOrder(5)]
		public int Bedroom; //房
		[FieldOrder(6)]
		public int Hall; //廳
		[FieldOrder(7)]
		public int Bathroom; //衛
		[FieldOrder(8)]
		public int Balcony; //陽台

		[FieldOrder(9)]
		public double Age;
		[FieldOrder(10)]
		[FieldConverter(typeof(FloorConverter))]
		public int? FloorFrom; //樓層
		[FieldOrder(11)]
		public int? FloorTo; //樓層
		[FieldOrder(12)]
		[FieldConverter(typeof(FloorConverter))]
		public int? MaxFloor; //最高樓層
		[FieldOrder(13)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string Orientation = ""; //朝向
		[FieldOrder(14)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string Community = ""; //社區
		[FieldOrder(15)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string Address = "";
		[FieldOrder(16)]
		public double Lat;
		[FieldOrder(17)]
		public double Lng;
		[FieldOrder(18)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string BuildingType = "";
		[FieldOrder(19)]
		public string BuildingUsage = ""; //法定用途

		[FieldOrder(20)]
		public double PingOfBuilding; //權狀坪數
		[FieldOrder(21)]
		public double PingOfIndoor; //主建物坪數
		[FieldOrder(22)]
		public double PingOfShared; //共用坪數
		[FieldOrder(23)]
		public double PingOfAncillary; //附屬建物坪數
		[FieldOrder(24)]
		public double PingOfBalcony; //陽台坪數
		[FieldOrder(25)]
		public double PingOfLand; //土地坪數

		[FieldOrder(26)]
		[FieldConverter(ConverterKind.Boolean, "Y", "")]
		public bool IncludeParking;
		[FieldOrder(27)]
		[FieldQuoted('"', QuoteMode.OptionalForBoth, MultilineMode.AllowForRead)]
		public string ParkingType = "";
		[FieldOrder(28)]
		public double PingOfParking; //車位坪數

		[FieldOrder(29)]
		public string PhotoDirectory = "";
		[FieldOrder(30)]
		public string HouseLink = "";

		[FieldOrder(31)]
		[FieldConverter(ConverterKind.Date, "yyyy-MM-dd HH:mm:ss")]
		public DateTime? CreatTime;

		[FieldHidden]
		public List<string> PhotoLinks;
	}
}
