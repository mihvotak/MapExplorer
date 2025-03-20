using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class BossLootExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		Dictionary<string, int> _parts = new Dictionary<string, int>();
		int counter = 0;
		int partsCounter = 0;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			foreach (var boss in dunge.Bosses)
			{
				if (boss.Loot.Count > 0)
				{
					List<string> tds = new List<string>();
					tds.Add(dunge.DungeLine.GetBossLink(boss.Num));
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(boss.Name.ToString());
					tds.Add(boss.Abils.Count.ToString());
					tds.Add(boss.AllAbilsStr.ToString());
					tds.Add(boss.Loot.Count.ToString());
					tds.Add(string.Join("|", boss.Loot));
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
					counter++;
					foreach (var part in boss.LootParts)
					{
						if (part == "ошмёток")
							continue;
						if (!_parts.ContainsKey(part))
							_parts.Add(part, 0);
						_parts[part]++;
						partsCounter++;
					}
				}
			}
			ReportProgress(i);
		}
		builder.AppendLine("Total dunges: " + counter);
		foreach (var pair in _parts)
			builder.AppendLine($"{pair.Key}\t{pair.Value}\t{pair.Value / (float)partsCounter}");
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/BossLootExplorer.txt", exploreRes);
		TableText = exploreRes;
	}
}
