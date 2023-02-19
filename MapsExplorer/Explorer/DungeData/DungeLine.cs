using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class DungeLine
	{
		public string Hash;
		public DungeKind Kind;
		public Category Category;
		public bool Custom;
		public bool Vault;
		public List<string> Gods = new List<string>();
		public bool Success;
		public DateTime DateTime;

		public string Link { get { return "https://gv.erinome.net/duels/log/" + Hash; } }
		public string GetBossLink(int bossNum) { return Link + "?boss=" + bossNum; }

		public override string ToString()
		{
			return Hash + "\t" + Kind + "\t" + (Category == Category.Рандом ? "" : ((int)Category).ToString()) + "\t" + (Vault ? "v" : "") + "\t" + string.Join<string>(", ", Gods) + "\t" + (Success ? "успех" : "провал") + "\t" + Utils.GetDateAndTimeString(DateTime);
		}
	}
}
