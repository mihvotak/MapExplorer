using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class VoicesAndPartsExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		Dictionary<string, int> _parts = new Dictionary<string, int>();
		int counter = 0;
		int partsCounter = 0;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			foreach (var boss in dunge.Bosses)
			{
				if (boss.PartVoices.Count > 0)
				{
					List<string> tds = new List<string>();
					tds.Add(dunge.LogLine.GetBossLink(boss.Num));
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(boss.Name.ToString());
					tds.Add(boss.Abils.Count.ToString());
					tds.Add(boss.AllAbilsStr.ToString());
					tds.Add(boss.LootParts.Count.ToString());
					tds.Add(string.Join("|", boss.LootParts));
					tds.Add(boss.PartVoices.Count.ToString());
					tds.Add(string.Join("|", boss.PartVoices));
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
