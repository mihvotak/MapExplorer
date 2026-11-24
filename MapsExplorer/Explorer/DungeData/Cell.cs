using MapsExplorer.Explorer.DungeData;

namespace MapsExplorer
{
	public class Cell
	{
		public CellKind CellKind;
		public bool PossibleBoss;
		public BossWarning BossWarning = new BossWarning();
		public Boss Boss;
		public Hint Hint;
		public HintCategory HintCategory;
		public string Symbol;
		public int Step = -1;
		public bool Reverse = false;
        public bool IsWall => CellKind == CellKind.Wall;
        public bool IsNotWall => CellKind != CellKind.Wall;

        public bool CanBeBoss => CellKind == CellKind.PossibleBoss || CellKind == CellKind.Something || CellKind == CellKind.UnknownNotWall || CellKind == CellKind.Unknown;
        public bool IsBossOrCanBe => CellKind == CellKind.Boss || CanBeBoss;

		public const string HintsList = "↑→↓←↗↘↙↖∨<∧>⌊⌈⌉⌋╩╠╦╣✵❄☁♨☀✺";

		public string GetSymbol(int rotation)
		{
			if (CellKind == CellKind.Unknown || CellKind == CellKind.UnknownNotWall)
				return "?";
			else if (CellKind == CellKind.Empty)
				return ".";
			else if (CellKind == CellKind.Wall)
				return "#";
			else if (CellKind == CellKind.Something)
				return "!";
			else if (CellKind == CellKind.Enter)
				return "В";
			else if (CellKind == CellKind.PossibleBoss)
				return "&";
			else if (CellKind == CellKind.Boss)
				return "Б" + Boss.BossPower;
			else if (CellKind == CellKind.Trap)
				return "-";
			else if (CellKind == CellKind.SecretRoom)
				return "X";
			else if (CellKind == CellKind.Teleport)
				return "~";
			else if (CellKind == CellKind.End)
				return "@";
			else if (CellKind == CellKind.Hint)
			{
				int index = (int)Hint;
				if (index < 16)
				{
					int fourPart = index % 4;
					int unchanged = index - fourPart;
					fourPart += rotation;
					fourPart = fourPart % 4;
					index = unchanged + fourPart;
				}
				return HintsList.Substring(index, 1);
			}
			else
				return " ";
		}

	}
}
