using GeoCoordinatePortable;
using HouseCrawlerCSharp.Model;
using NLog;
using System;
using System.Text.RegularExpressions;

namespace HouseCrawlerCSharp.Library
{
	static class StringHelper
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static string RemoveLineBreak(this string text, bool doTrim = false)
		{
			var output = text.Replace("\n", "").Replace("\r", "");

			if(doTrim)
			{
				output = output.Trim();
			}

			return output;
		}

		public static string RemoveTag(this string text, bool remveSpace= false)
		{
			var output = text.Replace("\n", "").Replace("\r", "");
			//output = Regex.Replace(output, @"<span ([^>]*)>(.*)</span>", "");
			//output = Regex.Replace(output, @"<a ([^>]*)>(.*)</a>", "");
			//output = Regex.Replace(output, @"<(.*)>(.*)</(.*)>", "").Trim();

			// 移除無意義的隱藏tag
			output = Regex.Replace(output, @"<(.+)></\1>", "");
			// 移除HTML tag及裡面的內容
			output = Regex.Replace(output, @"<(.+?)(\s.+?)?>.*?</\1>", "");
			// 移除剩下的結尾tag
			output = Regex.Replace(output, @"</.+?>", "").Trim();

			if (remveSpace)
			{
				output.Replace(" ", "");
			}

			return output;
		}

		public static GeoCoordinate ToLatLng(this string text)
		{
			try
			{
				var match = Regex.Match(text, @"(-?\d+(\.\d+)?),\s*(-?\d+(\.\d+)?)");
				if (match.Success)
				{
					var latlng = match.Groups[0].Value.Split(",");
					return new GeoCoordinate(double.Parse(latlng[0]), double.Parse(latlng[1]));
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return null;
		}

		public static HousePattern ToHousePattern(this string text)
		{
			var output = new HousePattern();

			try
			{
				Match match;

				match = Regex.Match(text, @"\d+房");
				if (match.Success)
				{
					output.Bedroom = int.Parse(match.Groups[0].Value.Replace("房", ""));
				}

				match = Regex.Match(text, @"\d室");
				if (match.Success)
				{
					output.Room = int.Parse(match.Groups[0].Value.Replace("室", ""));
				}

				match = Regex.Match(text, @"\d+廳");
				if (match.Success)
				{
					output.Hall = int.Parse(match.Groups[0].Value.Replace("廳", ""));
				}

				match = Regex.Match(text, @"\d+衛");
				if (match.Success)
				{
					output.Bathroom = int.Parse(match.Groups[0].Value.Replace("衛", ""));
				}

				match = Regex.Match(text, @"\d+陽");
				if (match.Success)
				{
					output.Balcony = int.Parse(match.Groups[0].Value.Replace("陽", ""));
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return output;
		}
	}
}
