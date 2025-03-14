
using Godot;
using System;

namespace Flamme.common.enums;

public enum Cardinal
{
  North,
  East,
  South,
  West
}

public static class CardinalExtensions
{
  public static Cardinal Opposite(this Cardinal cardinal)
  {
    return cardinal switch
    {
      Cardinal.North => Cardinal.South,
      Cardinal.East => Cardinal.West,
      Cardinal.South => Cardinal.North,
      Cardinal.West => Cardinal.East,
      _ => throw new ArgumentOutOfRangeException(nameof(cardinal), cardinal, null)
    };
  }

  public static Vector2I ToVector(this Cardinal cardinal)
  {
    return cardinal switch
    {
      Cardinal.North => new Vector2I(0, -1),
      Cardinal.East => new Vector2I(1, 0),
      Cardinal.South => new Vector2I(0, 1),
      Cardinal.West => new Vector2I(-1, 0),
      _ => throw new ArgumentOutOfRangeException(nameof(cardinal), cardinal, null)
    };
  }

  public static int GetRotationTo(this Cardinal cardinal, Cardinal cardinal2)
  {
    // Calculate the difference in cardinal directions and map it to degrees (clockwise)
    // North = 0, East = 1, South = 2, West = 3.
    var fromIndex = (int)cardinal;
    var toIndex = (int)cardinal2;
    
    return (toIndex - fromIndex + 4) % 4 * 90;
  }
}


