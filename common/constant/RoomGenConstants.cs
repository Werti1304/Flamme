#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // RoomGenConstants.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Godot;

namespace Flamme.common.constant;

public static class RoomGenConstants
{
  public const int RoofTerrainSourceId = 25;
  public const int FloorTileSourceId = 30;

  public static readonly Vector2I UnderPropFloor = new Vector2I(0, 4);
}
