using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class Paths
	{
		public const string BaseDir = "../../../../Base";
		public const string ResultsDir = "../../../../Results";
		public const string SaveDir = "../../../../Save";

		public static string GetDungeLogPath(string hash)
		{
			return "https://gv.erinome.net/duels/log/" + hash;
		}

		public static string GetBossLogPath(string hash, int bossNum)
		{
			return GetDungeLogPath(hash) + "?boss=" + bossNum;
		}
	}
}
