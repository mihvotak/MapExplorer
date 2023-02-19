using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class AquasExplorer : ExplorerBase
{
	public override void Work()
	{
		bool showOne = _resultLines.Count == 1;
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			if (line.Category != Category.Аква)
				continue;
			if (!line.Success)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			Map map = dunge.Maps[0];
			if (map.BadRouteWalls)
				continue;
			Int2 treasureDelta = dunge.TreasurePos.Pos - map.EnterPos;
			bool circle = true;
			int walls = 0;
			if (map.IsLeftWall)
				walls++;
			if (map.IsRightWall)
				walls++;
			if (map.IsTopWall)
				walls++;
			if (map.IsBottomWall)
				walls++;
			if (walls < 3)
				continue;
			foreach (Step move in dunge.Moves)
			{
				Int2 movePos = move.Pos;
				if (!(movePos.x == 1 || movePos.x == map.Width - 2 || movePos.y == 1 || movePos.y == map.Height - 2))
					circle = false;
			}
			if (!circle)
				continue;
			bool positiveX = treasureDelta.x > 0;
			bool positiveY = treasureDelta.y > 0;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(dunge.WrongDetectedAqua ? "Аква!" : dunge.DungeLine.Category.ToString());
			tds.Add(dunge.TreasurePos.Pos.x + "");
			tds.Add(dunge.TreasurePos.Pos.y + "");
			tds.Add(line.Kind.ToString());
			tds.Add(treasureDelta.x + "");
			tds.Add(treasureDelta.y + "");
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");

			ReportProgress(i);
		}

		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/AquasTab.txt", exploreTab);
		TableText = exploreTab;

	}
}
