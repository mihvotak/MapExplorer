using System;
using System.Collections.Generic;

namespace MapsExplorer
{
	public class LogLine
	{
		public string Hash;
		public AvantureKind AvantureKind;
		public PolygonKind PolygonKind;
		public DungeKind DungeKind;
		public DungeKind DungeKind2;
        public SeaKind SeaKind;
        public SeaKind SeaKind2;
		public MapCategory Category;
		public bool Custom;
		public bool Vault;
		public List<string> Gods = new List<string>();
		public int SuccessCount;
		public bool Success;
		public int BitsSum;
		public DateTime DateTime;

		public string Link { get { return "https://gv.erinome.net/duels/log/" + Hash; } }
		public string GetBossLink(int bossNum) { return Link + "?boss=" + bossNum; }

		public override string ToString()
		{
			return Hash + "\t" + (AvantureKind == AvantureKind.Dungeon ? 
				DungeKind + (DungeKind2 == DungeKind.Неизвестное ? "" : ("/" + DungeKind2)) + "\t" + (Category == MapCategory.Рандом ? "" : ((int)Category).ToString()) + "\t" + (Vault ? "v" : "") + "\t" + (Success ? "успех" : "провал") : 
				AvantureKind == AvantureKind.Sea ? "Море"
                    + (SeaKind == SeaKind.Обычное ? "" : " " + SeaKind)
                    + (SeaKind2 == SeaKind.Неизвестное ? "" : "/" + SeaKind2)
                    + "\t" + SuccessCount : PolygonKind + "") 
				+ "\t" + string.Join<string>(", ", Gods) + "\t" + Utils.GetDateAndTimeString(DateTime);
		}
	}
}
