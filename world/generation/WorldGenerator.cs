using Flamme.common.enums;
using Flamme.world.rooms;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Flamme.world.generation;

public partial class WorldGenerator : Node2D
{
  // TODO: Add actual generation logic here
  public RandomNumberGenerator NotSeedRng = new RandomNumberGenerator();

  public const string RoomFolderPath = "world/rooms/";
  public Dictionary<RoomType, List<RoomData>> RoomDict = new Dictionary<RoomType, List<RoomData>>();

  public struct RoomData
  {
    public PackedScene RoomScene;
    public string Name;
    public RoomSize Size;
    public RoomType Type;
    public RoomExit AllowedExits;
    public int RoomGenerationTickets;

    public RoomData(PackedScene roomScene, string name, RoomSize size, RoomType type, RoomExit allowedExits, int roomGenerationTickets)
    {
      RoomScene = roomScene;
      Name = name;
      Size = size;
      Type = type;
      AllowedExits = allowedExits;
      RoomGenerationTickets = roomGenerationTickets;
    }
  }

  public override void _Ready()
  {
    foreach (var roomType in Enum.GetValues<RoomType>())
    {
      RoomDict[roomType] = new List<RoomData>();
    }
    
    var dir = DirAccess.Open(RoomFolderPath);
    Debug.Assert(dir != null, "Room folder not found!");

    dir.ListDirBegin();

    foreach (var file in Directory.GetFiles(RoomFolderPath, "*.tscn", SearchOption.AllDirectories))
    {
      var roomScene = GD.Load<PackedScene>(file);
      var roomTemp = roomScene.Instantiate<Room>();
      var roomData = new RoomData(
        roomScene, roomTemp.Name, roomTemp.Size, roomTemp.Type, roomTemp.AllowedExits, roomTemp.RoomGenerationTickets);
      RoomDict[roomTemp.Type].Add(roomData);
      roomTemp.QueueFree();
    }
    return;
  }

  public enum LevelFloor
  {
    Level0,
    Level1
  }

  public Dictionary<LevelFloor, Level> Levels = new Dictionary<LevelFloor, Level>();
  public const string LevelScenePath = "world/generation/Level.tscn";

  public bool GeneratingFirstLevel = true;

  public void GenerateLevels()
  {
    var levelScene = GD.Load<PackedScene>(LevelScenePath);
    
    // Transition to level 0
    // TODO Do this after pressing button in main menu instead of Main.cs
    GetTree().CallDeferred("change_scene_to_packed", levelScene);
  }

  public void GenerateFirstLevel(Level level)
  {
    // Good enough for now as theres only 1 spawn
    var spawnData = RoomDict[RoomType.Spawn][0];

    Debug.Assert(level == GetTree().CurrentScene);
    var spawn = spawnData.RoomScene.Instantiate<Room>();
    level.AddChild(spawn);
    
    // TODO Add World Generation here
  }

  public WorldGenerator()
  {
    _instance = this;
  }
  
  public static WorldGenerator Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static WorldGenerator _instance;
  private static readonly object Padlock = new object();
}