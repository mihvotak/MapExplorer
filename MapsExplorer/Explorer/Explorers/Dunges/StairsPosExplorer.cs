using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class StairsPosExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		Plot2d plot = new Plot2d();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			//if (line.Category != Category.Аква)
			//	continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			Map map = dunge.Maps[0];
			if (map.StairsPos == Int2.Zero)
				continue;
			bool match = dunge.LookAsAqua;
			if (line.Category != MapCategory.Аква && !dunge.LookAsAqua)
				continue;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(line.Category.ToString());
			tds.Add(dunge.LookAsAqua ? "Аква!" : (dunge.LookAsStable ? "Конюшня!" : ""));
			tds.Add(line.DungeKind.ToString());
			tds.Add(map.Width.ToString());
			tds.Add(map.Height.ToString());
			Int2 delta = map.StairsPos - map.EnterPos;
			int x = delta.x;
			int y = -delta.y;
			tds.Add(x.ToString());
			tds.Add(y.ToString());
			tds.Add(dunge.FirstStairMove.ToString());
			tds.Add(dunge.LastFloor.ToString());
			if (match)
				plot.Inc(x, y);
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/StairsPosResult.txt", exploreRes);
		string s = plot.GetRes(20);
		s += plot.GetRes4(20);
		TableText = exploreRes;
		ResultText = s;
	}
}
