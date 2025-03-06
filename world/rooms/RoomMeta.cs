#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // RoomMeta.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Flamme.common.constant;
using Flamme.common.enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Flamme.world.rooms;

public class RoomMeta
{
  public static readonly Dictionary<RoomType, List<RoomMeta>> RoomDict 
    = new Dictionary<RoomType, List<RoomMeta>>();
  
  public PackedScene RoomScene;
  public string Name;
  public RoomSize Size;
  public RoomType Type;
  public RoomExit AllowedExits;
  public int RoomGenerationTickets;

  public RoomMeta(PackedScene roomScene, string name, RoomSize size, RoomType type, RoomExit allowedExits, int roomGenerationTickets)
  {
    RoomScene = roomScene;
    Name = name;
    Size = size;
    Type = type;
    AllowedExits = allowedExits;
    RoomGenerationTickets = roomGenerationTickets;
  }

  public static RoomMeta GetRandomRoom(RoomType roomType, RoomSize roomSize, RoomExit roomExits)
  {
    var list = RoomDict[roomType];

    var validTickets = 0;
    var validRooms = list.FindAll(delegate(RoomMeta r)
    {
      bool cond = r.Size == roomSize && (r.AllowedExits & roomExits) >= roomExits;
      if (cond)
      {
        validTickets += r.RoomGenerationTickets;
      }
      return cond;
    });
    if (validRooms.Count == 0)
    {
      GD.PushError($"No room of type {roomType} with size {roomSize} and exits {roomExits} found!");
      return null;
    }

    RoomMeta chosenRoom = null;
    var randomTicket = GD.RandRange(0, validTickets);
    var idx = 0;
    while (randomTicket >= 0)
    {
      var room = validRooms[idx];
      randomTicket -= room.RoomGenerationTickets;
      if (randomTicket <= 0)
      {
        chosenRoom = room;
        break;
      }
      idx++;
    }
    
    return chosenRoom;
  }

  /// <summary>
  /// Fills RoomDict, must only be called once
  /// </summary>
  public static void Init()
  {
    if (RoomDict.Count != 0)
    {
      GD.PushError("RoomMeta init() called twice!");
      return;
    }
    
    foreach (var roomType in Enum.GetValues<RoomType>())
    {
      RoomDict[roomType] = new List<RoomMeta>();
    }
    
    var dir = DirAccess.Open(PathConstants.RoomFolderPath);
    Debug.Assert(dir != null, "Room folder not found!");

    dir.ListDirBegin();

    // TODO Better ways probably exist - but this is the easiest one 
    // Horribly performance but for now, that's okay
    foreach (var file in Directory.GetFiles(PathConstants.RoomFolderPath, "*.tscn", SearchOption.AllDirectories))
    {
      var roomScene = GD.Load<PackedScene>(file);

      Room roomTemp;
      try
      {
        roomTemp = roomScene.Instantiate<Room>();
      }
      catch (InvalidCastException e)
      {
        Console.WriteLine($"Non-room type file found in room folder: {file}\n{e}");
        throw;
      }
      
      var roomData = new RoomMeta(
        roomScene, roomTemp.Name, roomTemp.Size, roomTemp.Type, roomTemp.AllowedExits, roomTemp.RoomGenerationTickets);
      RoomDict[roomTemp.Type].Add(roomData);
      roomTemp.QueueFree();
    }
  }
}
