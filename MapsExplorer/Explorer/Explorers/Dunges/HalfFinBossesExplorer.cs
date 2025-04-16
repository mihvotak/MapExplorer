using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class HalfFinBossesExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			if (dunge == null)
				break;
			int count = 0;
			string s = "";
			foreach (Boss boss in dunge.Bosses)
			{
				if (boss.Pos.Floor == 2 && boss.Abils.Count == 3)
				{
					count++;
					if (count > 1)
						s = dunge.LogLine.GetBossLink(boss.Num);
				}
			}
			{
				List<string> tds = new List<string>();
				tds.Add(line.Hash);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(line.Category.ToString());
				tds.Add(line.DungeKind.ToString());
				tds.Add(dunge.Steps.ToString());
				tds.Add(dunge.Members.Count.ToString());
				tds.Add(count.ToString());
				tds.Add(s);
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");
			}
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/HalfFinBosses.txt", exploreRes);
		TableText = exploreRes;

	}
}
