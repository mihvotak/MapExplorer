using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System;

public class HeroDamageExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();


		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			//if (line.Category != Category.Usual)
			//	continue;
			if (line.Kind == DungeKind.Бесшумности || line.Kind == DungeKind.Загадки || line.Kind2 == DungeKind.Бесшумности)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			if (dunge.StartKind == DungeKind.Бесшумности || dunge.StartKind == DungeKind.Загадки || dunge.StartKind2 == DungeKind.Бесшумности)
				continue;
			if (dunge.SecretRom.Exists && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark))
				continue;
			for (int ii = 0; ii < dunge.Bosses.Count; ii++)
			{
				var boss = dunge.Bosses[ii];
				if (boss.Num == 0)
					continue;
				if (boss.Abils.Contains(Ability.зовущий) || boss.Abils.Contains(Ability.мнимый) || boss.Abils.Contains(Ability.мутирующий) || boss.Abils.Contains(Ability.спешащий))
					continue;
				//if (boss.Name[0] == '+')
				//	continue;
				int steps = boss.Hps.Count;
				int members0 = boss.Hps[0].Length;
				for (int stI = 0; stI < steps; stI++)
				{
					if (stI == 0)
						continue;
					int st = stI - 1;
					if (st % 2 == 0) // ход босса
						continue;
					if (boss.InfluencesByStep[st] > 0)
						continue;
					var hps = boss.Hps[st + 1];
					var lastHps = boss.Hps[st];
					int members = hps.Length;
					if (members != members0)
						break;
					int bossHp = hps[members - 1];
					if (bossHp == 0)
						break;
					int indexAlive = -1;
					int aliveHeroes = 0;
					for (int k = 0; k < hps.Length - 1; k++)
					{
						if (lastHps[k] > 1)
						{
							if (indexAlive == -1)
								indexAlive = k;
							aliveHeroes++;
						}
					}
					if (aliveHeroes != 1)
						continue;
					int lastBossHp = lastHps[members - 1];
					int bossDelta = bossHp - lastBossHp;
					if (boss.TextLines[st] == null)
						continue;
					if (boss.TextLines[st].Count != 1)
						break;
						//builder.Append($"Wrong lines count={boss.TextLines[st].Count} in boss {line.GetBossLink(boss.Num)}&s={st} step {st}\n");

					//if (bossDelta != 0)
					//	continue;
					List<string> tds = new List<string>();
					tds.Add(dunge.StartKind.ToString() + (dunge.StartKind2 != DungeKind.Обыденности ? "+" + dunge.StartKind2 : ""));
					tds.Add($"{line.GetBossLink(boss.Num)}&s={st}");
					tds.Add(st.ToString());
					Step step = boss.Pos;
					//tds.Add(boss.Pos.Floor + "floor");
					tds.Add(dunge.Members[indexAlive].God);
					tds.Add(dunge.Members[indexAlive].Hero);
					tds.Add(dunge.Members[indexAlive].Hp.ToString());
					tds.Add(lastHps[indexAlive].ToString());
					tds.Add((hps[indexAlive] - lastHps[indexAlive]).ToString());
					tds.Add(boss.Name);
					tds.Add(boss.Abils.Count.ToString());
					tds.Add(boss.AllAbilsStr);
					tds.Add(boss.Hp.ToString());
					tds.Add(lastBossHp.ToString());
					tds.Add(bossDelta.ToString());
					tds.Add(boss.TextLines[st][0]);
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
				}
			}
			ReportProgress(i);
		}

		builder.Append("\n");
		builder.Append($"Dunges\t{_resultLines.Count}\n");
		builder.Append("\n");

		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/HeroDamageResult.txt", exploreRes);
		TableText = exploreRes;

	}
}
