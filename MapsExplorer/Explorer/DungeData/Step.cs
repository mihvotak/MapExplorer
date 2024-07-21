using System;
using System.Runtime.CompilerServices;

namespace MapsExplorer
{
	public struct Step
	{
		public int Floor;
		public Int2 Pos;
		public Int2 Delta;

		public Step(int x, int y, int floor, Int2 delta)
		{
			Pos = new Int2(x, y);
			Floor = floor;
			Delta = delta;
		}

		public Step(Int2 pos, int floor, Int2 delta)
		{
			Pos = pos;
			Floor = floor;
			Delta = delta;
		}

		public static bool operator ==(Step value1, Step value2)
		{
			return value1.Floor == value2.Floor && value1.Pos == value2.Pos;
		}

		public static bool operator !=(Step value1, Step value2)
		{
			return value1.Floor != value2.Floor || value1.Pos != value2.Pos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Step rhs) { return Floor == rhs.Floor && Pos == rhs.Pos; }

		public override bool Equals(object o) { return Equals((Step)o); }


		public override int GetHashCode()
		{
			return Pos.GetHashCode() + Floor * 10000;
		}
	}
}