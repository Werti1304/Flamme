using Flamme.common.constant;
using Flamme.common.enums;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Flamme.world.rooms;

[Tool]
public partial class RoomManager : Node2D
{
  [Export]
  public bool UpdateRoomScenes
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set
    {
      if(value)
        UpdateRoomScenesList();
    }
  }
  
  [Export] public Array<PackedScene> AllRoomScenes;

  public override void _Ready()
  {
    GD.Print($"Adding all {AllRoomScenes.Count} rooms to RoomDict...");
    FillRoomMetaDict();
    GD.Print("Done!");
  }
  
  private void UpdateRoomScenesList()
  {
    var dir = DirAccess.Open(PathConstants.RoomFolderPath);
    Debug.Assert(dir != null, "Room folder not found!");
    if (AllRoomScenes == null)
    {
      AllRoomScenes = new Array<PackedScene>();
    }
    
    dir.ListDirBegin();
    
    AllRoomScenes.Clear();

    GD.Print("Updating room scenes...");
    foreach (var file in Directory.GetFiles(PathConstants.RoomFolderPath, "*.tscn", SearchOption.AllDirectories))
    {
      var relativePath = file.Replace(PathConstants.RoomFolderPath, string.Empty);
      if (!relativePath.Contains("/") && !relativePath.Contains("\\"))
        continue; // Skip files not at least 1 level deep

      var roomScene = GD.Load<PackedScene>(file);

      if (!AllRoomScenes.Contains(roomScene))
      {
        GD.Print($"Adding room scene: {roomScene.ResourcePath}");
        AllRoomScenes.Add(roomScene);
      }
    }
    GD.Print("Done!");
  }

  private void FillRoomMetaDict()
  {
    RoomMeta.RoomDict.Clear();

    foreach (var roomType in Enum.GetValues<RoomType>())
    {
      RoomMeta.RoomDict[roomType] = new List<RoomMeta>();
    }

    // TODO Better ways probably exist - but this is the easiest one 
    // Horribly performance but for now, that's okay
    foreach (var roomScene in AllRoomScenes)
    {
      Room roomTemp;

      var nodeTemp = roomScene.Instantiate();
      roomTemp = nodeTemp as Room;

      if (roomTemp == null)
      {
        // For other scenes, which aren't rooms, but are still in some subfolder (tools, etc.)
        nodeTemp.QueueFree();
        continue;
      }

      var roomData = new RoomMeta(
        roomScene, roomTemp.Name, roomTemp.Type, roomTemp.TheoreticalDoorMarkers.Keys, roomTemp.RoomGenerationTickets,
        roomTemp.RestrictToFloor, roomTemp.LevelFloor);
      RoomMeta.RoomDict[roomTemp.Type].Add(roomData);
      roomTemp.QueueFree();
    }
  }

  private static RoomManager _instance;
  private static readonly object Padlock = new();
  
  public RoomManager()
  {
    _instance = this;
  }
  
  public static RoomManager Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
}