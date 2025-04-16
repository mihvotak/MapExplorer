using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class TreasurePosExplorer : ExplorerBase
{
	public override void Work()
	{
		int needFloor = 2;
		bool from1FloorEntrance = false;
		StringBuilder builder = new StringBuilder();
		Plot2d plot = new Plot2d();
		Plot2d plotSF = new Plot2d();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			if (!line.Success || line.Vault)
				continue;
			if (needFloor == 1 && line.Category != MapCategory.Рандом)
				continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			if (dunge.LastFloor != needFloor)
				continue;
			if (!line.Success || line.Vault)
				continue;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(line.DungeKind.ToString());
			tds.Add(dunge.Steps.ToString());
			Map map0 = dunge.Maps[0];
			Map map = dunge.Maps[dunge.LastFloor - 1];
			tds.Add(map.Width.ToString());
			tds.Add(map.Height.ToString());
			tds.Add(dunge.Bosses.Count.ToString());
			Int2 delta = dunge.TreasurePos.Pos - map.EnterPos;
			if (from1FloorEntrance)
				delta += (map0.StairsPos - map0.EnterPos);
			int x = delta.x;
			int y = -delta.y;
			tds.Add(x.ToString());
			tds.Add(y.ToString());
			plot.Inc(x, y);
			var xx = Math.Abs(x);
			var yy = Math.Abs(y);
			tds.Add((xx <= yy ? xx : yy) + ";" + (xx <= yy ? yy : xx));
			tds.Add(dunge.TreasureBetweenWalls ? "|x|" : "-");
			tds.Add(dunge.TreasureWalls.ToString());
			tds.Add(dunge.TreasureSchemeKind.ToString());
			tds.Add(dunge.TreasureScheme.ToString());
			bool normalFinPos = dunge.StartKind != DungeKind.Миграции && dunge.StartKind != DungeKind.Миграции && (!dunge.SecretRom.Exists || (dunge.SecretRom.SecretKind != SecretKind.ChangeType && dunge.SecretRom.SecretKind != SecretKind.UnknownMark));
			if (normalFinPos && dunge.SfinPos.Floor > 0)
			{
				Int2 pos = dunge.SfinPos.Pos - map.EnterPos;
				pos.y = -pos.y;
				tds.Add(pos.x.ToString());
				tds.Add(pos.y.ToString());
				plotSF.Inc(pos.x, pos.y);
				Int2 pos2 = dunge.SfinPos.Pos - dunge.TreasurePos.Pos;
				pos2.y = -pos2.y;
				tds.Add(Math.Abs(pos2.x) + "_" + Math.Abs(pos2.y));
				tds.Add(pos2.x.ToString());
				tds.Add(pos2.y.ToString());
			}
			else
			{
				tds.Add("-");
				tds.Add("-");
				tds.Add("|");
				tds.Add("-");
				tds.Add("-");
			}
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/TreasurePosResult.txt", exploreRes);
		string s = plot.GetRes(10);
		s += plot.GetRes4(10);
		s += plot.GetRes8(10);
		s += plotSF.GetRes(10);

		TableText = exploreRes;
		ResultText = s;
	}
}
