using System;
using System.Runtime.CompilerServices;

namespace MapsExplorer
{
	public struct Step
	{
		public int Floor;
		public Int2 Pos;

		public Step(int x, int y, int floor)
		{
			Pos = new Int2(x, y);
			Floor = floor;
		}

		public Step(Int2 pos, int floor)
		{
			Pos = pos;
			Floor = floor;
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

		public override bool Equals(object o) { return Equals((Int2)o); }


		public override int GetHashCode()
		{
			return Pos.GetHashCode() + Floor * 10000;
		}
	}
}