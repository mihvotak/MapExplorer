using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

public class BossesByNameExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		int counter = 0;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			foreach (var boss in dunge.Bosses)
			{
				if (boss.Name.Contains("Микро-"))
				{
					List<string> tds = new List<string>();
					tds.Add(dunge.LogLine.GetBossLink(boss.Num));
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(line.Category.ToString());
					tds.Add(line.DungeKind.ToString());
					tds.Add(boss.Name.ToString());
					tds.Add(boss.Abils.Count.ToString());
					tds.Add(boss.AllAbilsStr.ToString());
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
					counter++;
				}
			}
			if (dunge.Hps[dunge.Hps.Count - 1].Count(hp => hp > 1) == 1)
			{
				List<string> tds = new List<string>();
				tds.Add(dunge.LogLine.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(line.Category.ToString());
				tds.Add(line.DungeKind.ToString());
				tds.Add("Всего 1 выживший");
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");
				counter++;
			}
			ReportProgress(i);
		}
		builder.AppendLine("Total: " + counter);
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/BossesByName.txt", exploreRes);
		TableText = exploreRes;
	}
}
