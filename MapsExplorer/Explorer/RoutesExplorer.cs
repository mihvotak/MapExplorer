using System;
using System.Collections.Generic;

namespace MapsExplorer
{
	public class RoutesExplorer
	{
		public class RouteCell
		{
			public Int2 Pos;
			public Cell Cell;
			public int MinStep = -1;
			public List<int> MinPathLens = new List<int>();
			public List<Int2> MinFrom = new List<Int2>();
			public int RealStep = -1;
			public List<int> RealPathLens = new List<int>();
			public List<Int2> RealFrom = new List<Int2>();
			public List<bool> IsMinRoute = new List<bool>();
			public List<bool> IsRealRoute = new List<bool>();
		}

		public class RouteGrid
		{
			private RouteCell[,] _grid;

			public RouteGrid(int width, int height)
			{
				_grid = new RouteCell[width + 2, height + 2];
			}

			public RouteCell GetCell(Int2 pos)
			{
				return GetCell(pos.x, pos.y);
			}

			public RouteCell GetCell(int x, int y)
			{
				return _grid[x + 1, y + 1];
			}

			public void SetCell(int x, int y, RouteCell cell)
			{
				_grid[x + 1, y + 1] = cell;
			}
		}

		public void Explore(Dunge dunge)
		{
			Map map = dunge.Maps[dunge.LastFloor - 1];
			RouteGrid grid1 = new RouteGrid(map.Width, map.Height);
			RouteGrid grid2 = new RouteGrid(map.Width, map.Height);
			map.Grid = grid1;
			List<RouteCell> waiting = new List<RouteCell>();
			List<RouteCell> done = new List<RouteCell>();
			int xLeft = map.IsLeftWall ? 0 : -1;
			int xRight = map.IsRightWall ? map.Width - 1 : map.Width;
			int yTop = map.IsTopWall ? 0 : -1;
			int yBottom = map.IsBottomWall ? map.Height - 1 : map.Height;
			void CalculateSteps(Int2 inPos, RouteGrid gridI)
			{
				Cell enterCell = map.GetCell(map.EnterPos);
				RouteCell first = new RouteCell() { Cell = enterCell, Pos = inPos, MinStep = 0, RealStep = 0 };
				gridI.SetCell(inPos.x, inPos.y, first);
				waiting.Add(first);
				while (waiting.Count > 0)
				{
					RouteCell from = waiting[0];
					waiting.RemoveAt(0);
					bool wallkableFrom = from.Cell.CellKind != CellKind.Unknown && from.Cell.CellKind != CellKind.Wall;
					int realSteps = from.RealStep + 1;
					int minSteps = from.MinStep + 1;
					for (int dirI = 0; dirI < 4; dirI++)
					{
						Int2 dir = Int2.FourDirections[dirI];
						Int2 pos = from.Pos + dir;
						if (pos.x < xLeft || pos.x > xRight)
							continue;
						if (pos.y < yTop || pos.y > yBottom)
							continue;
						int reversI = (dirI + 2) % 4;
						bool needAddToWaiting = false;
						if (gridI.GetCell(pos.x, pos.y) == null)
						{
							Cell cell = pos.x >= 0 && pos.x < map.Width && pos.y >= 0 && pos.y < map.Height ? map.GetCell(pos.x, pos.y) : new Cell() { CellKind = CellKind.Unknown };
							gridI.SetCell(pos.x, pos.y, new RouteCell() { Pos = pos, Cell = cell });
							needAddToWaiting = true;
						}
						RouteCell rc = gridI.GetCell(pos.x, pos.y);
						if (rc.Cell.CellKind == CellKind.Wall)
							continue;

						if (rc.MinStep == -1 || rc.MinStep > minSteps)
						{
							rc.MinStep = minSteps;
							rc.MinFrom.Clear();
							rc.MinFrom.Add(from.Pos);
							needAddToWaiting = true;
						}
						else if (rc.MinStep == minSteps)
							rc.MinFrom.Add(from.Pos);

						if (wallkableFrom && from.RealStep >= 0)
						{
							if (rc.RealStep == -1 || rc.RealStep > realSteps)
							{
								rc.RealStep = realSteps;
								rc.RealFrom.Clear();
								rc.RealFrom.Add(from.Pos);
								needAddToWaiting = true;
							}
							else if (rc.RealStep == realSteps)
								rc.RealFrom.Add(from.Pos);
						}

						if (needAddToWaiting)
							waiting.Add(rc);
					}
				}
			}
			Int2 enterPos = map.EnterPos;
			CalculateSteps(enterPos, grid1);
			CalculateSteps(dunge.TreasurePos.Pos, grid2);

			// Mark min and real routes
			List<Int2> wayPoints = new List<Int2>();
			wayPoints.Add(dunge.TreasurePos.Pos);
			/*if (dunge1.DungeLine.Kind != DungeKind.Миграции && dunge1.DungeLine.Kind != DungeKind.Загадки)
			{
				foreach (Boss boss in dunge1.Bosses)
					if (boss.Abils.Count == 2)
						wayPoints.Add(boss.Pos);
			}*/
			for (int wayPointI = 0; wayPointI < wayPoints.Count; wayPointI++)
			{
				Int2 wayPointPos = wayPoints[wayPointI];
				if (grid1.GetCell(wayPointPos.x, wayPointPos.y) == null)
				{
					if (wayPointI == 0)
					{
						map.BadRouteWalls = true;
						return;
					}
					else
						continue;
				}
				bool hasRealPath = grid1.GetCell(wayPointPos.x, wayPointPos.y).RealStep > -1;
				if (!hasRealPath) // Жаль исключать из рассмотрения
				{
					if (wayPointI == 0)
					{
						map.BadRouteWalls = true;
						return;
					}
					else
						continue;
				}
				for (int x = xLeft; x <= xRight; x++)
				{
					for (int y = yTop; y <= yBottom; y++)
					{
						RouteCell cell = grid1.GetCell(x, y);
						if (cell != null)
						{
							cell.IsRealRoute.Add(false);
							cell.RealPathLens.Add(-1);
							cell.IsMinRoute.Add(false);
							cell.MinPathLens.Add(-1);
						}
					}
				}

				int realPathLen = -1;
				int minPathLen = -1;
				{                       //	Real
					done.Clear();
					RouteCell rc = grid1.GetCell(wayPointPos.x, wayPointPos.y);
					int pathLen = rc.RealStep;
					rc.IsRealRoute[wayPointI] = true;
					waiting.Add(rc);
					while (waiting.Count > 0)
					{
						rc = waiting[0];
						waiting.RemoveAt(0);
						foreach (Int2 fromPos in rc.RealFrom)
						{
							RouteCell from = grid1.GetCell(fromPos.x, fromPos.y);
							from.IsRealRoute[wayPointI] = true;
							if (from.Pos != map.EnterPos && !waiting.Contains(from) && !done.Contains(from))
								waiting.Add(from);
						}
						done.Add(rc);
						rc.RealPathLens[wayPointI] = pathLen;
					}
					realPathLen = pathLen;

					//	Min

					rc = grid1.GetCell(wayPointPos.x, wayPointPos.y);
					pathLen = rc.MinStep;
					rc.IsMinRoute[wayPointI] = true;
					waiting.Add(rc);
					done.Clear();
					while (waiting.Count > 0)
					{
						rc = waiting[0];
						waiting.RemoveAt(0);
						foreach (Int2 fromPos in rc.MinFrom)
						{
							RouteCell from = grid1.GetCell(fromPos);
							from.IsMinRoute[wayPointI] = true;
							if (from.Pos != map.EnterPos && !waiting.Contains(from) && !done.Contains(from))
								waiting.Add(from);
						}
						rc.MinPathLens[wayPointI] = pathLen;
						done.Add(rc);
					}
					minPathLen = pathLen;
				}

				foreach (Boss boss in dunge.Bosses)
				{
					if (boss.Pos.Floor != dunge.LastFloor)
						continue;
					Int2 pos = boss.Pos.Pos;
					RouteCell rc = grid1.GetCell(pos.x, pos.y);
					Int2 treasurePos = dunge.TreasurePos.Pos;
					Int2 diff = pos - treasurePos;
					if (Math.Abs(diff.x) <= 1 && Math.Abs(diff.y) <= 1
						&& (diff.x == 0 || diff.y == 0
							|| map.GetCell(pos.x, treasurePos.y).CellKind != CellKind.Wall
							|| map.GetCell(treasurePos.x, pos.y).CellKind != CellKind.Wall))
					{
						boss.IsFinal = true;
					}
					else if (rc.IsRealRoute[wayPointI] && rc.IsMinRoute[wayPointI])
						boss.IsRouting |= true;
					else if (rc.IsRealRoute[wayPointI] || rc.IsMinRoute[wayPointI])
						boss.CanBeRouting |= true;
					else
					{
						RouteCell rc1 = grid1.GetCell(pos);
						RouteCell rc2 = grid2.GetCell(pos);
						int pathLen = rc1.MinStep + rc2.MinStep;
						if (pathLen <= realPathLen)
						{
							bool canBeRouting = true;
							waiting.Add(rc1);
							done.Clear();
							while (waiting.Count > 0)
							{
								rc = waiting[0];
								waiting.RemoveAt(0);
								RouteCell rcIn2 = grid2.GetCell(rc.Pos);
								if (rcIn2.RealStep != -1)
								{
									int sum = rc.MinStep + rcIn2.RealStep;
									if (sum < pathLen)
									{
										canBeRouting = false;
										waiting.Clear();
										break;
									}
								}
								foreach (Int2 posFrom in rc.MinFrom)
								{
									RouteCell from = grid1.GetCell(posFrom);
									if (!waiting.Contains(from) && !done.Contains(from))
										waiting.Add(from);
								}
								done.Add(rc);
							}
							waiting.Add(rc2);
							done.Clear();
							while (waiting.Count > 0)
							{
								rc = waiting[0];
								waiting.RemoveAt(0);
								RouteCell rcIn1 = grid1.GetCell(rc.Pos);
								if (rcIn1.RealStep != -1)
								{
									int sum = rc.MinStep + rcIn1.RealStep;
									if (sum < pathLen)
									{
										canBeRouting = false;
										waiting.Clear();
										break;
									}
								}
								foreach (Int2 posFrom in rc.MinFrom)
								{
									RouteCell from = grid2.GetCell(posFrom);
									if (!waiting.Contains(from) && !done.Contains(from))
										waiting.Add(from);
								}
								done.Add(rc);
							}
							if (canBeRouting)
								boss.CanBeRouting = true;
						}
					}
				}
			}

		}
	}
}