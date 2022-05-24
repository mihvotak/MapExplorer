using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[System.Serializable]
public struct Int2
{
	public int x;
	public int y;

	public Int2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static readonly Int2 MinCell = new Int2(int.MinValue, int.MinValue);
	public static readonly Int2 Zero = new Int2(0, 0);
	public static readonly Int2 One = new Int2(1, 1);
	public static readonly Int2[] FourDirections = new Int2[] { new Int2(0, -1), new Int2(1, 0), new Int2(0, 1), new Int2(-1, 0) };
	public static readonly Int2[] FiveDirs = new Int2[] { Int2.Zero, new Int2(1, 0), new Int2(0, 1), new Int2(-1, 0), new Int2(0, -1) };
	public static readonly Int2[] EightDirs = new Int2[] { new Int2(1, -1), new Int2(1, 0), new Int2(1, 1), new Int2(0, 1), new Int2(-1, 1), new Int2(-1, 0), new Int2(-1, -1), new Int2(0, -1) }; 
	public static readonly Int2[] ZeroDirs = new Int2[] { new Int2(0, 0) };
	public static readonly Int2[] TwoDirsY = new Int2[] { new Int2(0, 1), new Int2(0, -1) };
	public static readonly Int2[] TwoDirsX = new Int2[] { new Int2(1, 0), new Int2(-1, 0) };

	public double magnitude { get { return Math.Sqrt(x * x + y * y); } }

	public int DiagonalMagnitude { get { return Math.Max(Math.Abs(x), Math.Abs(y)); } }

	public int SumMagnitude { get { return Math.Abs(x) + Math.Abs(y); } }

	public Int2 NormalizedBy4Directions
	{
		get
		{
			return Math.Abs(x) >= Math.Abs(y) ? new Int2(x > 0 ? 1 : -1, 0) : new Int2(0, y > 0 ? 1 : -1);
		}
	}

	public Int2 NormalizedBy8Directions
	{
		get
		{
			Int2 result = new Int2(0, 0);
			if (x != 0 || y != 0)
			{
				int absX = Math.Abs(x);
				int absY = Math.Abs(y);
				if (absX >= absY)
				{
					result.x = x > 0 ? 1 : -1;
					float ratio = (float)y / (float)absX;
					if (ratio > .5f)
						result.y = 1;
					else if (ratio < -.5f)
						result.y = -1;
				}
				else
				{
					result.y = y > 0 ? 1 : -1;
					float ratio = (float)x / (float)absY;
					if (ratio > .5f)
						result.x = 1;
					else if (ratio < -.5f)
						result.x = -1;
				}
			}
			return result;
		}
	}

	public static int DiagonalDistance(Int2 value0, Int2 value1)
	{
		return Math.Max(Math.Abs(value1.x - value0.x), Math.Abs(value1.y - value0.y));
	}

	public static double Distance(Int2 value0, Int2 value1)
	{
		Int2 delta = value1 - value0;
		return Math.Sqrt(delta.x * delta.x + delta.y * delta.y);
	}

	public static Int2 operator +(Int2 value0, Int2 value1)
	{
		return new Int2(value0.x + value1.x, value0.y + value1.y);
	}

	public static Int2 operator -(Int2 value0, Int2 value1)
	{
		return new Int2(value0.x - value1.x, value0.y - value1.y);
	}

	public static Int2 operator -(Int2 value)
	{
		return new Int2(-value.x, -value.y);
	}

	public static Int2 operator *(Int2 value0, int value1)
	{
		return new Int2(value0.x * value1, value0.y * value1);
	}

	public static Int2 operator *(int value0, Int2 value1)
	{
		return new Int2(value0 * value1.x, value0 * value1.y);
	}

	public static bool operator ==(Int2 value1, Int2 value2)
	{
		return value1.x == value2.x && value1.y == value2.y;
	}

	public static bool operator !=(Int2 value1, Int2 value2)
	{
		return value1.x != value2.x || value1.y != value2.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Int2 rhs) { return x == rhs.x && y == rhs.y; }

	public override bool Equals(object o) { return Equals((Int2)o); }

	public override int GetHashCode()
	{
		return x ^ y;
	}

	override public string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
