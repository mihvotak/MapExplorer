using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class RoutesAndBossesExplorer : ExplorerBase
{
	public override void Work()
	{
		bool showOne = _resultLines.Count == 1;
		int half = 20;
		int halfHalf = 10;
		int[,] noRoutes = new int[half * 2, half * 2];
		int[,] routes = new int[half * 2, half * 2];
		int[,] canRoutes = new int[half * 2, half * 2];
		int[,] all1 = new int[half * 2, half * 2];
		StringBuilder builder = new StringBuilder();
		StringBuilder builder2 = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			if (line.Category != Category.Usual)
				continue;
			if (!line.Success || line.Vault)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			Map map = dunge.Maps[dunge.LastFloor - 1];
			if (map.BadRouteWalls)
				continue;
			if (dunge.WrongDetectedAqua || !dunge.IsNormalBosses())
				continue;
			//if (!dunge.AllBossesFound)
			//	continue;
			Int2 treasureDelta = dunge.TreasurePos.Pos - map.EnterPos;
			bool positiveX = treasureDelta.x > 0;
			bool positiveY = treasureDelta.y > 0;
			foreach (Boss boss in dunge.Bosses)
			{
				List<string> tds = new List<string>();
				if (boss.Pos.Floor != dunge.LastFloor)
					continue;
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(dunge.WrongDetectedAqua ? "Aqua!" : dunge.DungeLine.Category.ToString());
				tds.Add(line.Kind.ToString());
				Int2 delta = boss.Pos.Pos - map.EnterPos;
				tds.Add(delta.x + "");
				tds.Add(delta.y + "");
				tds.Add(boss.Name);
				Int2 delta2 = delta;
				if (!positiveX)
					delta2.x = -delta2.x;
				if (!positiveY)
					delta2.y = -delta2.y;
				tds.Add(delta2.x + "");
				tds.Add(delta2.y + "");

				bool hasWalls = (positiveY ? map.IsTopWall : map.IsBottomWall) && (positiveX ? map.IsLeftWall : map.IsRightWall);
				if (!hasWalls) continue;
				int bossFromWallY = positiveY ? boss.Pos.Pos.y : map.Height - boss.Pos.Pos.y - 1;
				int bossFromWallX = positiveX ? boss.Pos.Pos.x : map.Width - boss.Pos.Pos.x - 1;
				int enterFromWallY = positiveY ? map.EnterPos.y : map.Height - map.EnterPos.y - 1;
				int enterFromWallX = positiveX ? map.EnterPos.x : map.Width - map.EnterPos.x - 1;
				bool closestX = bossFromWallX < bossFromWallY;
				int bossFromWall = closestX ? bossFromWallX : bossFromWallY;
				int enterFromWall = closestX ? enterFromWallX : enterFromWallY;
				tds.Add(hasWalls ? "wall" : "no");
				tds.Add(bossFromWall + "");
				tds.Add(enterFromWall + "");
				tds.Add("|");
				tds.Add(boss.Abils.Count + "");
				tds.Add(boss.IsFinal ? "Финальный" : (boss.IsRouting ? "Путевой" : (boss.CanBeRouting ? "Пут?" : (delta2.x > 0 && delta2.y > 0 ? "Блуждун" : (delta2.x < 0 && delta2.y < 0 ? "Угловой" : "Пристен")))));
				tds.Add(dunge.Bosses.Count + "");
				if ((boss.Abils.Count == 1 || boss.Abils.Count == 0) && !boss.IsFinal)
				//if (boss.Abils.Count == 1)
				{
					if (!boss.IsRouting && !boss.CanBeRouting)
						noRoutes[delta2.x + half, delta2.y + half]++;
					else if (boss.IsRouting)
						routes[delta2.x + half, delta2.y + half]++;
					else if (boss.CanBeRouting)
						canRoutes[delta2.x + half, delta2.y + half]++;
					all1[delta2.x + half, delta2.y + half]++;
				}

				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");

			}
			if (showOne)
			{
				for (int y = 0; y < map.Height; y++)
				{
					for (int x = 0; x < map.Width; x++)
					{
						Cell cell = map.GetCell(x, y);
						string sym = cell.GetSymbol(dunge.Rotation);
						RoutesExplorer.RouteCell rc = map.Grid.GetCell(x, y);
						bool calculatedBoss = cell.CellKind == CellKind.Boss && cell.Boss.BossPower == 0;
						if (calculatedBoss)
							sym = "Б!";
						int waiPointIndex = 0;
						if (rc != null)
						{
							if (_checkBoxMinRouteChecked)
							{
								if (rc.MinPathLens[waiPointIndex] > -1 && cell.CellKind != CellKind.Boss && cell.CellKind != CellKind.Wall)
									sym = rc.MinPathLens[waiPointIndex] + "";
								if (rc.IsMinRoute[waiPointIndex])
									sym = "[" + sym + "]";
							}
							else
							{
								if (rc.RealPathLens[waiPointIndex] > -1 && cell.CellKind != CellKind.Boss && cell.CellKind != CellKind.Wall)
									sym = rc.RealPathLens[waiPointIndex] + "";
								if (rc.IsRealRoute[waiPointIndex])
									sym = "[" + sym + "]";
							}
						}
						builder.Append(sym + "\t");
					}
					builder.Append("\n");
				}
			}
			ReportProgress(i);
		}

		void WriteResultGraph(string name, int[,] data)
		{
			builder2.Append(name + "\n");
			data[half, half] = -1;
			builder2.Append("\t" + "\t");
			for (int x = -halfHalf; x <= halfHalf; x++)
				builder2.Append(x + "\t");
			builder2.Append("\n");
			for (int y = -halfHalf; y <= halfHalf; y++)
			{
				builder2.Append("\t" + y + "\t");
				for (int x = -halfHalf; x <= halfHalf; x++)
					builder2.Append(data[x + half, y + half] + "\t");
				builder2.Append("\n");
			}
			builder2.Append("\n");
		}
		WriteResultGraph("Не путевые", noRoutes);
		WriteResultGraph("Путевые", routes);
		WriteResultGraph("Возможно путевые", canRoutes);
		WriteResultGraph("Все однушки", all1);

		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/RoutesTab.txt", exploreTab);
		TableText = exploreTab;

		string exploreRes = builder2.ToString();
		File.WriteAllText(Paths.ResultsDir + "/RoutesResult.txt", exploreRes);
		ResultText = exploreRes;
	}
}
