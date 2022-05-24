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
			DateTime time = DateTime.ParseExact(dateStr, DateTimeFormat, _provider);
			return time;

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
