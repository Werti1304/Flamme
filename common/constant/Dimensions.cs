#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // Dimensions.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Flamme.common.enums;
using Godot;
using System;
using System.Collections.Generic;

namespace Flamme.common.constant;

public static class Dimensions
{
  // Defines how big the different room sizes are in pixels
  // This can't change anyways without changing every room, so no config neccessary
  public static readonly Godot.Collections.Dictionary<RoomSize, Vector2I> RoomSizeDict = new Godot.Collections.Dictionary<RoomSize, Vector2I>( ){
    { RoomSize.S1X1, new Vector2I(17, 11)},
    { RoomSize.S1X2, new Vector2I(17, 20)},
    { RoomSize.S2X1, new Vector2I(32, 11)},
    { RoomSize.S2X2, new Vector2I(32, 20)},
    { RoomSize.S3X1, new Vector2I(47, 20)},
    { RoomSize.S1X3, new Vector2I(17, 29)},
    { RoomSize.S3X3, new Vector2I(47, 29)}
  };
  
  public static Vector2I GetExitPosition(RoomSize size, RoomExit exit)
  {
    return exit switch
    {
      RoomExit.North =>  new Vector2I(8, 0),
      RoomExit.South =>  new Vector2I(8, RoomSizeDict[size].Y - 1),
      RoomExit.West =>   new Vector2I(0, 5),
      RoomExit.East =>   new Vector2I(RoomSizeDict[size].X - 1, 5),
      RoomExit.North2 => new Vector2I(23, 0),
      RoomExit.South2 => new Vector2I(23, RoomSizeDict[size].Y - 1),
      RoomExit.West2 =>  new Vector2I(0, 14),
      RoomExit.East2 =>  new Vector2I(RoomSizeDict[size].X - 1, 14),
      RoomExit.North3 => new Vector2I(38, 0),
      RoomExit.South3 => new Vector2I(38, RoomSizeDict[size].Y - 1),
      RoomExit.West3 =>  new Vector2I(0, 23),
      RoomExit.East3 =>  new Vector2I(RoomSizeDict[size].X - 1, 23),
      _ => throw new ArgumentOutOfRangeException(nameof(exit), exit, null)
    };
  }

  public static readonly Dictionary<RoomExit, RoomExit> OppositeExits = new Dictionary<RoomExit, RoomExit>
  {
    { RoomExit.North, RoomExit.South },
    { RoomExit.South, RoomExit.North },
    { RoomExit.West, RoomExit.East },
    { RoomExit.East, RoomExit.West },
    { RoomExit.North2, RoomExit.South2 },
    { RoomExit.South2, RoomExit.North2 },
    { RoomExit.West2, RoomExit.East2 },
    { RoomExit.East2, RoomExit.West2 },
    { RoomExit.North3, RoomExit.South3 },
    { RoomExit.South3, RoomExit.North3 },
    { RoomExit.West3, RoomExit.East3 },
    { RoomExit.East3, RoomExit.West3 }
  };
}
