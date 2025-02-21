using Godot;
using Godot.Collections;
using System;

namespace Flamme.testing;

public partial class TestRoom1 : Room
{
  public Dictionary<DoorPos, Vector2I> DoorTileMapDict = new Dictionary<DoorPos, Vector2I>()
  {
    { DoorPos.North, new Vector2I(7, 0) },
    { DoorPos.South, new Vector2I(7, 8) },
    { DoorPos.West, new Vector2I(0, 4) },
    { DoorPos.East, new Vector2I(14, 4) },
  };
  
  public override Type GetRoomType()
  {
    return Type.S1X1;
  }

  public override DoorPos GetDoorPositions()
  {
    return DoorPos.North | DoorPos.South | DoorPos.West | DoorPos.East;
  }

  public override void Open(DoorPos doorPos)
  {
    ChangeDoor(doorPos, 0, new Vector2I(3, 0));
  }

  public override void Close(DoorPos doorPos)
  {
    ChangeDoor(doorPos, 0, new Vector2I(3, 1));
  }

  protected override Dictionary<DoorPos, Vector2I> GetDoorTileMapDict()
  {
    return DoorTileMapDict;
  }
}