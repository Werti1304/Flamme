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
        OnRoomChanged();
    }
  }
  
  [Export] public Array<PackedScene> AllRoomScenes;

  public override void _Ready()
  {
    GD.Print(AllRoomScenes.Count);
  }

  public void OnRoomChanged()
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