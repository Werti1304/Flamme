using Flamme.common.constant;
using Flamme.common.enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Flamme.world.rooms;

public class RoomMeta(
  PackedScene roomScene,
  string name,
  RoomType type,
  ICollection<Cardinal> possibleExits,
  int roomGenerationTickets,
  bool restrictToFloor,
  LevelFloor levelFloor = LevelFloor.Prison1)
{
  public static readonly Dictionary<RoomType, List<RoomMeta>> RoomDict
    = new Dictionary<RoomType, List<RoomMeta>>();

  public PackedScene RoomScene = roomScene;
  public string Name = name;
  public RoomType Type = type;
  public ICollection<Cardinal> PossibleExits = possibleExits;
  public int RoomGenerationTickets = roomGenerationTickets;
  public bool RestrictToFloor = restrictToFloor;
  public LevelFloor LevelFloor = levelFloor;

  public static RoomMeta GetRandomRoom(RoomType roomType, ICollection<Cardinal> exitsNeeded, LevelFloor levelFloor)
  {
    var list = RoomDict[roomType];

    var validTickets = 0;
    var validRooms = list.FindAll(delegate(RoomMeta r)
    {
      foreach (var cardinal in exitsNeeded)
      {
        if (!r.PossibleExits.Contains(cardinal) || (r.RestrictToFloor && r.LevelFloor != levelFloor))
        {
          return false;
        }
      }
      validTickets += r.RoomGenerationTickets;
      return true;
    });

    if (validRooms.Count == 0)
    {
      GD.PushError($"No room of type {roomType} with exits {exitsNeeded} found!");
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
}
