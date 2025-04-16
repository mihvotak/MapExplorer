using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class LogLine
	{
		public string Hash;
		public AvantureKind AvantureKind;
		public PolygonKind PolygonKind;
		public DungeKind DungeKind;
		public DungeKind DungeKind2;
		public MapCategory Category;
		public bool Custom;
		public bool Vault;
		public List<string> Gods = new List<string>();
		public bool Success;
		public int BitsSum;
		public DateTime DateTime;

		public string Link { get { return "https://gv.erinome.net/duels/log/" + Hash; } }
		public string GetBossLink(int bossNum) { return Link + "?boss=" + bossNum; }

		public override string ToString()
		{
			return Hash + "\t" + (AvantureKind == AvantureKind.Dungeon ? 
				DungeKind + (DungeKind2 == DungeKind.Неизвестное ? "" : ("/" + DungeKind2)) + "\t" + (Category == MapCategory.Рандом ? "" : ((int)Category).ToString()) + "\t" + (Vault ? "v" : "") + "\t" + (Success ? "успех" : "провал") : 
				PolygonKind + "") 
				+ "\t" + string.Join<string>(", ", Gods) + "\t" + Utils.GetDateAndTimeString(DateTime);
		}
	}
}
