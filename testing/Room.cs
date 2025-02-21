#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // Room.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Godot;
using Godot.Collections;
using System;

namespace Flamme.testing;

public abstract partial class Room : Area2D
{
  [ExportGroup("Meta")] 
  [Export] public TileMapLayer TileMap;
  [Export] public CollisionShape2D CollisionShape;
  
  public enum Type
  {
    S1X1,
    S1X2,
    S2X1,
    S2X2,
    S3X1,
    S1X3,
    S3X3
  }

  [Flags]
  public enum DoorPos
  {
    None   = 0,
    North  = 0x1,   // 1
    South  = 0x2,   // 2
    West   = 0x4,   // 4
    East   = 0x8,   // 8
    North2 = 0x10, // 16
    South2 = 0x20, // 32
    West2  = 0x40, // 64
    East2  = 0x80, // 128
    North3 = 0x100, // 256
    South3 = 0x200, // 512
    West3  = 0x400, // 1024
    East3  = 0x800,  // 2048
    Size   = 0x801
  }

  public override void _Ready()
  {
    ZIndex = -1;
    ExportMetaNonNull.Check(this);
  }

  public abstract Type GetRoomType();
  public abstract DoorPos GetDoorPositions();
  public abstract void Open(DoorPos doorPos);
  public abstract void Close(DoorPos doorPos);

  protected abstract Dictionary<DoorPos, Vector2I> GetDoorTileMapDict();

  public void OpenAll()
  {
    Open(GetDoorPositions());
  }

  public void CloseAll()
  {
    Close(GetDoorPositions());
  }

  protected void ChangeDoor(DoorPos doorPos, int sourceID, Vector2I atlasCoords)
  {
    if (doorPos == DoorPos.None)
    {
      GD.PushWarning($"Tried to open a door at NONE in room {Name}");
      return;
    }

    for (var i = 1; i < (int)DoorPos.Size; i <<= 1)
    {
      if ((doorPos & (DoorPos)i) == 0)
        continue;

      if (!GetDoorTileMapDict().TryGetValue(doorPos, out var doorCoords))
      {
        GD.PushWarning($"Tried to open a nonexistent door at {doorPos} none in room {Name}");
        return;
      }
      
      TileMap.SetCell(doorCoords, sourceID, atlasCoords);
      return;
    }
  }
}
