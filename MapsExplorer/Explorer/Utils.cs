using System;
using System.Globalization;

namespace MapsExplorer
{
	public class Utils
	{
		private static CultureInfo _provider = CultureInfo.InvariantCulture;
		private static string DateTimeFormat = "dd.MM.yyyy HH:mm";
		private static string DateFolderFormat = "yyyy.MM.dd";

		public static DateTime ParseDateTime(string dateStr)
		{
			if (string.IsNullOrEmpty(dateStr))
				return DateTime.MinValue;
			try
			{
				if (DateTime.TryParseExact(dateStr, DateTimeFormat, _provider, DateTimeStyles.None, out DateTime time))
					return time;
			}
			catch
			{
				// ignored
			}
			return DateTime.MinValue;
		}

		public static string GetDateFolderString(DateTime dateTime)
		{
			return dateTime.ToString(DateFolderFormat);
		}

		public static string GetDateAndTimeString(DateTime dateTime)
		{
			return dateTime.ToString(DateTimeFormat);
		}
	}
}
