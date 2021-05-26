using FileHelpers;
using System;

namespace HouseCrawlerCSharp.Model
{
	class FloorConverter : ConverterBase
	{
		public override object StringToField(string from)
		{
			if (from.EndsWith("F", StringComparison.OrdinalIgnoreCase))
			{
				return int.Parse(from.Replace("F", "", StringComparison.OrdinalIgnoreCase));
			}
			else if (from.StartsWith("B", StringComparison.OrdinalIgnoreCase))
			{
				return int.Parse(from.Replace("B", "", StringComparison.OrdinalIgnoreCase)) * -1;
			}

			return null;
		}

		public override string FieldToString(object from)
		{
			if(from == null)
			{
				return "";
			}

			var value = (int)from;
			if (value > 0)
			{
				return $"{value}F";
			}
			else if (value < 0)
			{
				return $"B{value * -1}";
			}

			return base.FieldToString(from);
		}
	}
}
