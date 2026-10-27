using System;
using System.Collections.Generic;
using System.Globalization;

// Кубические координаты X + Y + Z == 0.
public struct SeaPosition : IEquatable<SeaPosition>
{
    public int X;
    public int Y;
    public int Z;
    public int PortDistance => Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));

    public SeaPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public int DistanceTo(SeaPosition other)
    {
        return (Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z)) / 2;
    }

    public IEnumerable<SeaPosition> GetNeighbours()
    {
        yield return new SeaPosition(X + 1, Y - 1, Z);
        yield return new SeaPosition(X, Y - 1, Z + 1);
        yield return new SeaPosition(X - 1, Y, Z + 1);
        yield return new SeaPosition(X - 1, Y + 1, Z);
        yield return new SeaPosition(X, Y + 1, Z - 1);
        yield return new SeaPosition(X + 1, Y, Z - 1);
    }

    public bool Equals(SeaPosition other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object obj) => obj is SeaPosition && Equals((SeaPosition)obj);
    public override int GetHashCode() => (X * 397 ^ Y) * 397 ^ Z;
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", X, Y, Z);

    public static SeaPosition Parse(string value)
    {
        var parts = value.Split('_');
        if (parts.Length != 3)
            throw new FormatException("Invalid sea coordinates: " + value);
        var position = new SeaPosition(int.Parse(parts[0], CultureInfo.InvariantCulture),
            int.Parse(parts[1], CultureInfo.InvariantCulture), int.Parse(parts[2], CultureInfo.InvariantCulture));
        if (position.X + position.Y + position.Z != 0)
            throw new FormatException("Invalid sea cube coordinates: " + value);
        return position;
    }
}

public enum SeaCellKind
{
    Unknown, Sea, Island, Whirlpool, Port, Border, ClosedBorder, Lighthouse,
    SmallRift, LargeRift, Reefs, Ice
}

public enum SeaObjectKind
{
    None, Fish, WanderingFish, Giant, Leviathan, SummonedLeva
}

public enum SeaTreasureKind
{
    None, Treasure, Relic
}

public class SeaCell
{
    public SeaPosition Position;
    public SeaCellKind Kind;
    public SeaObjectKind ObjectKind;
    public SeaTreasureKind Treasure;
    public char Symbol;
    public char InterestHint;
    public List<string> ArkNumbers = new List<string>();
    public bool IsBubble => Kind == SeaCellKind.SmallRift || Kind == SeaCellKind.LargeRift;
    public bool IsBorder => Kind == SeaCellKind.Border || Kind == SeaCellKind.ClosedBorder;

    public SeaCell Copy()
    {
        var result = (SeaCell)MemberwiseClone();
        result.ArkNumbers = new List<string>(ArkNumbers);
        return result;
    }
}

public class SeaLogEntry
{
    public int StepNum;
    public string ArkNumber;
    public string Text;
}

public enum SeaRiftResult
{
    Unknown, Island, Whirlpool, Supplies, Repair, Leviathan,
    LeviathanHintIsland, SuppliesIsland, RepairIsland, PrayerIsland, HintIsland, MysteryIsland
}

public class SeaRiftActivation
{
    public int StepNum;
    public SeaPosition Position;
    public string ArkNumber;
    public SeaCellKind BubbleKind;
    public SeaRiftResult Result;
    public string Text;
    public SeaPosition? LeviathanPosition;
}
