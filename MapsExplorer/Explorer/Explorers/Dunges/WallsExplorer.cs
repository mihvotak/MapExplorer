using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class WallsExplorer : ExplorerBase
{
	public override void Work()
	{
		int maxSize = 22;
		int minSize = 9;
		string[,] aqua = new string[maxSize, maxSize];
		string[,] norm1f = new string[maxSize, maxSize];
		string[,] cust1f = new string[maxSize, maxSize];
		string[,] sum1f = new string[maxSize, maxSize];
		string[,] norm2f = new string[maxSize, maxSize];
		string[,] cust2f = new string[maxSize, maxSize];
		string[,] sum2f = new string[maxSize, maxSize];
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			//if (line.Category != Category.Aqua)
			//	continue;
			//if (!line.Success)
			//	continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
            if (dunge == null)
                continue;
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
			{
				if (wallsShure || aqua[map.Width, map.Height] == null)
					aqua[map.Width, map.Height] = line.Link;
			}
			else
			{
				if (dunge.LastFloor == 1)
				{
					if (dunge.IsCustom)
					{
						if (wallsShure || cust1f[map.Width, map.Height] == null)
							cust1f[map.Width, map.Height] = line.Link;
					}
					else
					{
						if (wallsShure || norm1f[map.Width, map.Height] == null)
							norm1f[map.Width, map.Height] = line.Link;
					}
					if (wallsShure || sum1f[map.Width, map.Height] == null)
						sum1f[map.Width, map.Height] = line.Link;
				}
				else if (dunge.LastFloor == 2)
				{
					if (dunge.IsCustom)
					{
						if (wallsShure || cust2f[map.Width, map.Height] == null)
							cust2f[map.Width, map.Height] = line.Link;
					}
					else
					{
						if (wallsShure || norm2f[map.Width, map.Height] == null)
							norm2f[map.Width, map.Height] = line.Link;
					}
					if (wallsShure || sum2f[map.Width, map.Height] == null)
						sum2f[map.Width, map.Height] = line.Link;
				}
			}
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");

			ReportProgress(i);
		}
		string PrintArray(string[,] array, string name)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(name);
			for (int w = minSize; w < maxSize; w++)
				sb.Append("\t" + w);
			sb.AppendLine();
			for (int h = minSize; h < maxSize; h++)
			{
				sb.Append(h + "");
				for (int w = minSize; w < maxSize; w++)
					sb.Append("\t" + array[w, h]);
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
