using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class WallsExplorer : ExplorerBase
{
	public override void Work()
	{
		bool showOne = _resultLines.Count == 1;
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			//if (line.Category != Category.Aqua)
			//	continue;
			//if (!line.Success)
			//	continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			Map map = dunge.Maps[dunge.LastFloor - 1];
			if (map.BadRouteWalls)
				continue;
			bool walls = map.IsLeftWall && map.IsRightWall && map.IsTopWall && map.IsBottomWall;
			if (!walls)
				continue;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add((dunge.LookAsAqua && dunge.DungeLine.Category != Category.Аква) ? "Аква!" : dunge.DungeLine.Category.ToString());
			tds.Add(line.Kind.ToString());
			tds.Add(dunge.LastFloor + "");
			tds.Add(map.Width + "");
			tds.Add(map.Height + "");
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");

			ReportProgress(i);
		}

		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/WallsTab.txt", exploreTab);
		TableText = exploreTab;
	}
}
