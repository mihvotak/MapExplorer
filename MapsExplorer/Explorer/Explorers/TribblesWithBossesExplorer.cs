using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System;

public class TribblesWithBossesExplorer : ExplorerBase
{
	public override void Work()
	{
		List<DateTime> tribbleDays = new List<DateTime>();
		/*tribbleDays.Add(new DateTime(2019, 12, 30));
		tribbleDays.Add(new DateTime(2020, 01, 02));
		tribbleDays.Add(new DateTime(2020, 01, 25));
		tribbleDays.Add(new DateTime(2020, 03, 11));
		tribbleDays.Add(new DateTime(2020, 03, 18));
		tribbleDays.Add(new DateTime(2020, 04, 04));
		tribbleDays.Add(new DateTime(2020, 04, 24));
		tribbleDays.Add(new DateTime(2020, 05, 21));
		tribbleDays.Add(new DateTime(2020, 06, 01));
		tribbleDays.Add(new DateTime(2020, 06, 06));*/

		var dateStrings = new string[] { "30.12.19", "02.01.20", "25.01.20", "24.04.20", "21.05.20", "01.06.20", "03.07.20", "09.07.20", "02.09.20", "21.11.20", "01.12.20", "28.12.20", "15.05.21", "20.06.21", "12.12.21", "12.01.22", "30.06.22", "11.03.20", "18.03.20", "04.04.20", "06.06.20", "19.07.20", "22.07.20", "17.08.20", "27.10.20", "29.11.20", "03.12.20", "27.01.21", "03.03.21", "18.05.21", "28.05.21", "11.07.21", "02.08.21", "21.08.21", "29.09.21", "25.11.21", "06.02.22", "15.02.22", "27.02.22", "22.06.22", "26.06.22", "19.08.22", "01.09.22" };
		DateTime date;
		foreach (string str in dateStrings)
		{
			bool parsed = DateTime.TryParseExact(str, "dd.MM.yy", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), System.Globalization.DateTimeStyles.None, out date);
			if (parsed)
				tribbleDays.Add(date);
		}

		StringBuilder builder = new StringBuilder();

		int tMax = 6;
		int[] tAll = new int[tMax];
		int[] tTribbles = new int[tMax];

		int abMax = 8;
		int[] abAll = new int[abMax];
		int[] abTribbles = new int[abMax];

		int total = 0;
		int loses = 0;
		int escapes = 0;
		int winNoEsc = 0;
		int tribbles = 0;
		int tribbles2 = 0;
		int mids = 0;
		int firsts = 0;

		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			//if (line.Category != Category.Usual)
			//	continue;
			//if (line.Hash != "apn8pky5p")
			//	continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			int killedBosses = dunge.Bosses.Count(b => b.HeroesWin);
			bool time1 = line.DateTime < new DateTime(2019, 10, 29);
			bool time2 = !time1 && line.DateTime < new DateTime(2019, 12, 27);

			for (int ii = 0; ii < dunge.Bosses.Count; ii++)
			{
				var boss = dunge.Bosses[ii];
				if (boss.Num == 0)
					continue;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(Utils.GetDateAndTimeString(boss.DateTime));
				int period = time1 ? 1 : (time2 ? 2 : 3);
				if (period == 3)
				{
					foreach (DateTime dateTime in tribbleDays)
						if (dateTime.Date == boss.DateTime.Date)
							period = 4;
				}
				if (boss.Pos.Floor == 2)
					period += 2;
				int ab = boss.Abils.Count;
				if (boss.Pos.Floor == 2)
					ab += 4;
				total++;
				if (!boss.HeroesWin)
					loses++;
				else if (boss.Escape1)
					escapes++;
				else
				{
					winNoEsc++;
					tAll[period - 1]++;
					abAll[ab - 1]++;
					if (boss.TribbleInFinal)
					{
						tribbles++;
						tTribbles[period - 1]++;
						abTribbles[ab - 1]++;
						if (boss.TribbleInFinal2)
							tribbles2++;
					}
				}

				if (boss.TribbleInMiddle)
					mids++;
				if (boss.TribbleInFirst)
					firsts++;
				tds.Add(period.ToString());
				tds.Add(line.Success ? (line.Vault ? "v" : "s") : "f");
				tds.Add(line.Category.ToString());
				//tds.Add(line.Kind.ToString());
				//tds.Add(line.Kind == DungeKind.Бесшумности ? "б" : "");
				//tds.Add(dunge.Steps.ToString());
				tds.Add(line.GetBossLink(boss.Num));
				tds.Add(boss.Num.ToString());
				tds.Add(dunge.Bosses.Count.ToString());
				tds.Add(killedBosses.ToString());
				//tds.Add(dunge.Members.Count.ToString());
				//tds.Add(dunge.MembersSumHp.ToString());
				tds.Add(boss.Abils.Count.ToString());
				tds.Add(boss.AllAbilsStr);
				//tds.Add(boss.Abils.Contains(Ability.мощный) ? "м" : "");
				//tds.Add(boss.Abils.Contains(Ability.мутирующий) ? "му" : "");
				tds.Add(boss.Name);
				//tds.Add(boss.Hp.ToString());
				//tds.Add("");
				//tds.Add((Math.Abs(posX)).ToString());
				//tds.Add((Math.Abs(posY)).ToString());
				tds.Add(boss.HeroesWin ? "win" : "lose");
				tds.Add(boss.TribbleInFinal ? (boss.TribbleInFinal2 ? "2" : "1") : "");
				tds.Add(boss.Escape1 ? "escape" : "");
				Step step = boss.Pos;
				tds.Add(step.Floor.ToString());
				Map map = dunge.Maps[step.Floor - 1];
				int posX = step.Pos.x - map.EnterPos.x;
				int posY = step.Pos.y - map.EnterPos.y;
				tds.Add(posX.ToString());
				tds.Add((-posY).ToString());
				tds.Add(boss.Pos.Floor + "floor");
				/*string posS = "";
				if (boss.Abils.Count == 1)
				{
					if ((Math.Abs(posX) > 5 || Math.Abs(posY) > 5) &&
						(!line.Success || line.Vault || (boss.Pos - dunge.TreasurePos).DiagonalMagnitude > 1))
						posS = "out";
					if (posX == 0 || posY == 0)
						posS = "in";
				}
				tds.Add(posS);
				*/
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");
			}
			ReportProgress(i);
		}

		builder.Append("\n");
		builder.Append($"Dunges\t{_resultLines.Count}\n");
		builder.Append($"Bosses\t{total}\n");
		builder.Append($"Lose\t{loses}\n");
		builder.Append($"Escape\t{escapes}\n");
		builder.Append($"Win\t{winNoEsc}\n");
		builder.Append($"Tribbles\t{tribbles}\t{((double)tribbles / winNoEsc * 100).ToString("f2")}%\n");
		builder.Append($"Pair of tribbles\t{tribbles2}\t{((double)tribbles2 / tribbles * 100).ToString("f2")}%\n");
		builder.Append("\n");
		builder.Append($"Middle\t{mids}\n");
		builder.Append($"First\t{firsts}\n");
		builder.Append("\n");

		builder.Append("\n");
		builder.Append("Period\t");
		for (int i = 1; i <= tMax; i++)
			builder.Append(i + "\t");
		builder.Append("\n");
		builder.Append("Wins\t");
		for (int i = 1; i <= tMax; i++)
			builder.Append(tAll[i - 1] + "\t");
		builder.Append("\n");
		builder.Append("Tribbles\t");
		for (int i = 1; i <= tMax; i++)
			builder.Append(tTribbles[i - 1] + "\t");
		builder.Append("\n");
		builder.Append("Percent\t");
		for (int i = 1; i <= tMax; i++)
			builder.Append(((double)tTribbles[i - 1] / tAll[i - 1] * 100).ToString("f2") + "%\t");
		builder.Append("\n");

		builder.Append("\n");
		builder.Append("Abils:\t");
		for (int i = 1; i <= abMax; i++)
			builder.Append(i + "\t");
		builder.Append("\n");
		builder.Append("Wins\t");
		for (int i = 1; i <= abMax; i++)
			builder.Append(abAll[i - 1] + "\t");
		builder.Append("\n");
		builder.Append("Tribbles\t");
		for (int i = 1; i <= abMax; i++)
			builder.Append(abTribbles[i - 1] + "\t");
		builder.Append("\n");
		builder.Append("Percent\t");
		for (int i = 1; i <= abMax; i++)
			builder.Append(((double)abTribbles[i - 1] / abAll[i - 1] * 100).ToString("f2") + "%\t");
		builder.Append("\n");

		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/TribblesResult.txt", exploreRes);
		TableText = exploreRes;

	}
}
