using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class Paths
	{
		public const string DungeonsBaseDir = "../../../../Base/Dungeons";
		public const string PolygonsBaseDir = "../../../../Base/Polygons";
		public const string ResultsDir = "../../../../Results";
		public const string SaveDir = "../../../../Save";

		public static string GetLogPath(string hash)
		{
			return "https://gv.erinome.net/duels/log/" + hash;
		}

		public static string GetBossLogPath(string hash, int bossNum)
		{
			return GetLogPath(hash) + "?boss=" + bossNum;
		}
	}
}
