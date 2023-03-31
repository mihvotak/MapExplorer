using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class Map
	{
		public int Width;
		public int Height;
		public List<Cell> Cells = new List<Cell>();

		public Int2 EnterPos;
		public Int2 StairsPos;
		
		public bool EnterCorner;	// Старт от угла
		public bool EnterWall;		// Старт от стены
		public Int2 EnterDir;		// Направление от начальной стены

		public bool IsRightWall;
		public bool IsLeftWall;
		public bool IsTopWall;
		public bool IsBottomWall;

		public bool RareWalls;

		public bool BadRouteWalls;
		public RoutesExplorer.RouteGrid Grid;

		public Cell GetCell(int x, int y)
		{
			int index = x + y * Width;
			return Cells[index];
		}

		public Cell GetCell(Int2 pos)
		{
			return GetCell(pos.x, pos.y);
		}
	}
}