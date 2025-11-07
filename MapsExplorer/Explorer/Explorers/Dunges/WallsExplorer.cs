using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class WallsExplorer : ExplorerBase
{
	public override void Work()
	{
		int[,] aqua = new int[21, 21];
		int[,] norm1f = new int[21, 21];
		int[,] cust1f = new int[21, 21];
		int[,] sum1f = new int[21, 21];
		int[,] norm2f = new int[21, 21];
		int[,] cust2f = new int[21, 21];
		int[,] sum2f = new int[21, 21];
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			//if (line.Category != Category.Aqua)
			//	continue;
			//if (!line.Success)
			//	continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			Map map = dunge.Maps[dunge.LastFloor - 1];
			if (map.BadRouteWalls)
				continue;
			if (dunge.LastFloor == 1 && (dunge.LogLine.Category == MapCategory.Конюшня || dunge.LogLine.Category == MapCategory.Ромашка))
				continue;
			bool walls = map.IsLeftWall && map.IsRightWall && map.IsTopWall && map.IsBottomWall;
			if (!walls)
				continue;
			bool wallsShure = map.IsShureLeftWall && map.IsShureRightWall && map.IsShureTopWall && map.IsShureBottomWall;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add((dunge.LookAsAqua && dunge.LogLine.Category != MapCategory.Аква) ? "Аква" : dunge.LogLine.Category.ToString());
			tds.Add(dunge.IsCustom ? "Кастомное" : "Обычное");
			tds.Add(wallsShure ? "Подробно" : "Нет");
			tds.Add(line.DungeKind.ToString());
			tds.Add(dunge.LastFloor + "Э");
			tds.Add(map.Width + "");
			tds.Add(map.Height + "");
			int probability = wallsShure ? 2 : 1;
			if (dunge.LastFloor == 1 && (dunge.LogLine.Category == MapCategory.Аква || dunge.LookAsAqua))
				aqua[map.Width, map.Height] = Math.Max(probability, aqua[map.Width, map.Height]);
			else
			{
				if (dunge.LastFloor == 1)
				{
					if (dunge.IsCustom)
						cust1f[map.Width, map.Height] = Math.Max(probability, cust1f[map.Width, map.Height]);
					else
						norm1f[map.Width, map.Height] = Math.Max(probability, norm1f[map.Width, map.Height]);
					sum1f[map.Width, map.Height] = Math.Max(probability, sum1f[map.Width, map.Height]);
				}
				else if (dunge.LastFloor == 2)
				{
					if (dunge.IsCustom)
						cust2f[map.Width, map.Height] = Math.Max(probability, cust2f[map.Width, map.Height]);
					else
						norm2f[map.Width, map.Height] = Math.Max(probability, norm2f[map.Width, map.Height]);
					sum2f[map.Width, map.Height] = Math.Max(probability, sum2f[map.Width, map.Height]);
				}
			}
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");

			ReportProgress(i);
		}
		string PrintArray(int[,] array, string name)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(name);
			for (int w = 10; w <= 16; w++)
				sb.Append("\t" + w);
			sb.AppendLine();
			for (int h = 10; h <= 16; h++)
			{
				sb.Append(h + "");
				for (int w = 10; w <= 16; w++)
				{
					int value = array[w, h];
					sb.Append("\t" + value);
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}

		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/WallsTab.txt", exploreTab);
		TableText = exploreTab;
		StringBuilder resSb = new StringBuilder();
		resSb.AppendLine(PrintArray(aqua, "Аквы"));
		resSb.AppendLine(PrintArray(norm1f, "Обычные, 1Э"));
		resSb.AppendLine(PrintArray(cust1f, "Кастомные, 1Э"));
		resSb.AppendLine(PrintArray(sum1f, "Все рандомные, 1Э"));
		resSb.AppendLine(PrintArray(norm2f, "Обычные, 2Э"));
		resSb.AppendLine(PrintArray(cust2f, "Кастомные, 2Э"));
		resSb.AppendLine(PrintArray(sum2f, "Все рандомные, 2Э"));
		ResultText = resSb.ToString();
		File.WriteAllText(Paths.ResultsDir + "/WallsResult.txt", ResultText);
	}
}
