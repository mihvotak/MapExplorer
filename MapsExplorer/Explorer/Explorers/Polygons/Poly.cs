public static class Poly
{
	public const string A = "A";
	public const string B = "B";
	public const string C = "C";
	public const string D = "D";

	public static Int2 Up = new Int2(0, -1);
	public static Int2 Down = new Int2(0, 1);
	public static Int2 Right = new Int2(1, 0);
	public static Int2 Left = new Int2(-1, 0);

	public static InfluenceKind GetInfluenceByStr(string infl)
	{
		if (infl == "e")
			return InfluenceKind.Encourage;
		if (infl == "p")
			return InfluenceKind.Punish;
		if (infl == "m")
			return InfluenceKind.Miracle;
		return InfluenceKind.None;
	}

	public static Int2 GetDirByArrow(string arrow)
	{
		if (arrow == "⇡")
			return Up;
		if (arrow == "⇣")
			return Up;
		if (arrow == "⇠")
			return Left;
		if (arrow == "⇢")
			return Right;
		return Int2.Zero;
	}
}
