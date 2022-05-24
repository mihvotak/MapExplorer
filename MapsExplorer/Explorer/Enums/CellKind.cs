using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public enum CellKind
	{
		Unknown,
		UnknownNotWall,
		Empty,
		Wall,
		Something,
		Enter,
		Trap,
		PossibleBoss,
		Boss,
		SecretRoom,
		Teleport,
		Hint,
		ClosedTreasure,
		Treasure,
		Storeroom,
		End,
		Stairs
	}
}
