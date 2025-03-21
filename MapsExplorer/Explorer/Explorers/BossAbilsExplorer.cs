using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class BossAbilsExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		List<Dictionary<Ability, int>> _abils1fl = new List<Dictionary<Ability, int>>();
		for (int i = 0; i <= 4; i++)
			_abils1fl.Add(new Dictionary<Ability, int>());
		List<Dictionary<Ability, int>> _abils2fl = new List<Dictionary<Ability, int>>();
		for (int i = 0; i <= 4; i++)
			_abils2fl.Add(new Dictionary<Ability, int>());
		int counter = 0;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			foreach (var boss in dunge.Bosses)
			{
				/*
				int steps = boss.Hps.Count;
				int stI = steps - 1;
				var hps = boss.Hps[stI];
				int members = hps.Length;
				int bossHp = hps[members - 1];
				int indexAlive = -1;
				int aliveHeroes = 0;
				for (int k = 0; k < hps.Length - 1; k++)
				{
					if (hps[k] > 1)
					{
						if (indexAlive == -1)
							indexAlive = k;
						aliveHeroes++;
					}
				}
				*/
				if (boss.Name[0] != '(' && (boss.AbilsCount > 2 || boss.Name[0] != '+')) 
				{
					List<string> tds = new List<string>();
					tds.Add(dunge.DungeLine.GetBossLink(boss.Num));
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(boss.Pos.Floor.ToString());
					tds.Add(boss.Name.ToString());
					tds.Add(boss.AbilsCount.ToString());
					tds.Add(boss.AllAbilsStr.ToString());
					tds.Add(string.Join("|", boss.Loot));


						string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
					counter++;
					var abils = boss.Pos.Floor == 1 ? _abils1fl[boss.AbilsCount] : _abils2fl[boss.AbilsCount];
					foreach (var abil in boss.Abils)
					{
						if (!abils.ContainsKey(abil))
							abils.Add(abil, 0);
						abils[abil]++;
						if (abil == Ability.крепчающий)
							break;
					}
				}
			}
			ReportProgress(i);
		}
		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/BossAbilsExplorer_table.txt", exploreTab);
		TableText = exploreTab;
		builder.Clear();
		builder.AppendLine("Total dunges: " + counter);
		for (int ai = 1; ai <= 3; ai++)
		{
			var abils = _abils1fl[ai];
			int count = 0;
			foreach (var pair in abils)
				count += pair.Value;
			foreach (var pair in abils)
				builder.AppendLine($"1э\t{ai}\t{pair.Key}\t{pair.Value}\t{pair.Value / (float)count}");
			builder.AppendLine("");
		}
		for (int ai = 1; ai <= 4; ai++)
		{
			var abils = _abils2fl[ai];
			int count = 0;
			foreach (var pair in abils)
				count += pair.Value;
			foreach (var pair in abils)
				builder.AppendLine($"2э\t{ai}\t{pair.Key}\t{pair.Value}\t{pair.Value / (float)count}");
			builder.AppendLine("");
		}
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/BossAbilsExplorer_result.txt", exploreTab);
		ResultText = exploreRes;
	}
}
