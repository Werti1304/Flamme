using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.entities.staff;
using Flamme.ui;
using Flamme.world.rooms;
using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using Godot.Collections;

namespace Flamme.world.generation;

public partial class WorldGenerator : Node2D
{
  public RandomNumberGenerator NotSeedRng = new RandomNumberGenerator();

  private static readonly Array<Vector2I> Neighbours = new Array<Vector2I>
  { // needs same order as RoomExit
    new Vector2I(0, -1), // north
    new Vector2I(0, 1),  // south
    new Vector2I(-1, 0), // west
    new Vector2I(1, 0),  // east
  };

  public override void _Ready()
  {
    RoomMeta.Init();
  }

  public System.Collections.Generic.Dictionary<LevelFloor, Level> Levels = new System.Collections.Generic.Dictionary<LevelFloor, Level>();

  public bool GeneratingFirstLevel = true;

  public void GenerateLevels()
  {
    var levelScene = GD.Load<PackedScene>(PathConstants.LevelScenePath);
    
    // Transition to level 0
    // TODO Do this after pressing button in main menu instead of Main.cs
    GetTree().CallDeferred("change_scene_to_packed", levelScene);
  }
  
  public void GenerateFirstLevel(Level level)
  {
    var levelSize = new Vector2I(level.Grid.GetLength(0), level.Grid.GetLength(1));
    var levelCenter = levelSize / 2;
    
    GD.Print("Generating spawn...");
    // Good enough for now as theres only 1 spawn
    var spawnData = RoomMeta.RoomDict[RoomType.Spawn][0];
    Debug.Assert(level == GetTree().CurrentScene);
    var spawn = spawnData.RoomScene.Instantiate<Room>();
    level.AddChild(spawn);
    level.Spawn = spawn;
    level.Grid[levelCenter.X, levelCenter.Y] = spawn;
    GD.Print($"Placing down spawn {spawn.Name} at {spawn.GlobalPosition}");
    
    GD.Print("Generating rooms...");
    GenerateRooms(level);
    GD.Print("Level generated!");
    
    GD.Print("Placing down character, camera & staff");
    var globalSpawnPosition = spawn.GetGlobalMidPoint();
    
    // TODO 1 Preload 
    var playerScene = GD.Load<PackedScene>(PathConstants.PlayerScenePath);
    var player = playerScene.Instantiate<PlayableCharacter>();
    player.GlobalPosition = globalSpawnPosition;
    level.AddChild(player);
    player.Owner = level;
    
    // ...
    var playerCameraScene = GD.Load<PackedScene>(PathConstants.PlayerCameraScenePath);
    var playerCamera = playerCameraScene.Instantiate<PlayerCamera>();
    playerCamera.GlobalPosition = globalSpawnPosition;
    level.AddChild(playerCamera);
    playerCamera.Player = player;
    playerCamera.Owner = level;
    
    // ...
    var startingStaffScene = GD.Load<PackedScene>(PathConstants.StartingStaffScenePath);
    var startingStaff = startingStaffScene.Instantiate<Staff>();
    startingStaff.GlobalPosition = globalSpawnPosition - new Vector2(64, 64);
    level.AddChild(startingStaff);
    startingStaff.Owner = level;
    
    // Update Minimap
    Hud.Instance.Minimap.Update(level);
  }

  private void GenerateRooms(Level level)
  {
    const int roomCount = 32;
    var levelSize = new Vector2I(level.Grid.GetLength(0), level.Grid.GetLength(1));
    var levelCenter = levelSize / 2;
    
    var weightGrid = new int[levelSize.X, levelSize.Y];
    // init weightGrid with -1
    for (var y = 0; y < levelSize.Y; y++)
    {
      for (var x = 0; x < levelSize.X; x++)
      {
        weightGrid[x, y] = -1;
      }
    }
    
    var potNeighbours = new List<(Vector2I position, int weight)>(); // List of potential neighbours
    var currRoomCount = 0;
    
    potNeighbours.Add((levelCenter, 0));  // add spawn to queue
    
    // generate paths
    while (potNeighbours.Count > 0)
    {
      var index = GetRandomWeighted(potNeighbours, level, levelCenter);
      var position = potNeighbours[index].position;
      var weight = potNeighbours[index].weight;
      
      // remove from queue
      potNeighbours.RemoveAt(index);
      weightGrid[position.X, position.Y] = weight;
      currRoomCount++;
      if (currRoomCount == roomCount) break;
      
      // remove invalid neighbours
      for (var i = 0; i < potNeighbours.Count; i++)
      {
        if ((weightGrid[potNeighbours[i].position.X, potNeighbours[i].position.Y] != -1) || // already filled
            (level.CountNeighbours(weightGrid, potNeighbours[i].position) > 1))             // too many neighbours
        {
          potNeighbours.RemoveAt(i);
          i--;
        }
      }
      
      // add neighbours to queue
      foreach (var pos in Neighbours)
      {
        var neighbour = position + pos;
        if ((!level.IsPosValid(neighbour)) ||                // out of bounds
            (weightGrid[neighbour.X, neighbour.Y] != -1) ||     // already filled
            (level.CountNeighbours(weightGrid, neighbour) > 1)) // too many neighbours
        {
          continue;
        }
          
        potNeighbours.Add((neighbour, weight+1));
      }
    }
    
    // room types that can replace end rooms, except boss room
    var endRoomTypes = new Array<RoomType>
    {
      RoomType.Treasure,
      RoomType.Shop,
      RoomType.Smithy,
      //RoomType.Secret,
    };
    
    var tileSize = new Vector2I(32, 32);
    var roomSize = new Vector2I(17, 11);
    
    // end rooms
    var hasBossRoom = false;
    var endRooms = GetEndRooms(ref weightGrid, levelCenter);
    var rounds = 100; // pfusch
    while (endRooms.Count > 0 && rounds-- > 0)
    {
      var randomEndRoomIndex = GD.RandRange(0, endRooms.Count-1);
      
      //get random element from RoomDict
      var roomType = hasBossRoom ? endRoomTypes[GD.RandRange(0, endRoomTypes.Count-1)] : RoomType.Boss; // choose boss room at first
      if (RoomMeta.RoomDict[roomType].Count == 0)  // if no room of this type exists remove it from list
      {
        endRoomTypes.Remove(roomType); //remove and try again
        continue;
      }
      var roomData = RoomMeta.RoomDict[roomType][GD.RandRange(0, RoomMeta.RoomDict[roomType].Count-1)]; // get random room of this type
      
      var exits = GetRoomExits(ref weightGrid, endRooms[randomEndRoomIndex]); // get exits for room
      var actualExits = (RoomExit)((int)exits & (int)roomData.AllowedExits);
      if (actualExits == 0) continue;
      if (roomType == RoomType.Boss) hasBossRoom = true;
      
      weightGrid[endRooms[randomEndRoomIndex].X, endRooms[randomEndRoomIndex].Y] = -(int)roomType; // change weight to room type for debugging
      
      var room = roomData.RoomScene.Instantiate<Room>();
      room.ActualExits = actualExits;

      var placeGlobalPos = (endRooms[randomEndRoomIndex] - levelCenter) * roomSize * tileSize;
      GD.Print($"Placing down room {room.Name} at {placeGlobalPos}, index {endRooms[randomEndRoomIndex]}");
      room.GlobalPosition = placeGlobalPos;
      level.AddChild(room);
      room.Owner = level;
      level.Grid[endRooms[randomEndRoomIndex].X, endRooms[randomEndRoomIndex].Y] = room;
    }
    
    //load paths and spawn
    for (var y = 0; y < level.Grid.GetLength(1); y++)
    {
      for (var x = 0; x < level.Grid.GetLength(0); x++)
      {
        if (level.Grid[x, y] != null || weightGrid[x, y] == -1) 
          continue;
        
        if (y == levelCenter.Y && x == levelCenter.X)
          continue;
        
        // var roomData = RoomMeta.RoomDict[RoomType.Pathway][0];
        var roomPos = new Vector2I(x, y);
        var actualExits = GetRoomExits(ref weightGrid, roomPos);
        var roomMeta = RoomMeta.GetRandomRoom(RoomType.Pathway, RoomSize.S1X1, actualExits);
        var room = roomMeta.RoomScene.Instantiate<Room>();
        var placeGlobalPos = (roomPos - levelCenter) * roomSize * tileSize;
        GD.Print($"Placing down room at {placeGlobalPos}");
        room.GlobalPosition = placeGlobalPos;
        room.ActualExits = actualExits; // get exits for room
        level.AddChild(room);
        room.Owner = level;
        level.Grid[x, y] = room;
      }
    }
    
    
    // print in console
    for (var y = 0; y < level.Grid.GetLength(1); y++)
    {
      var line = "";
      for (var x = 0; x < level.Grid.GetLength(0); x++)
      {
        var number = $"{weightGrid[x, y]}";
        // add leading 0s
        while (number.Length < 2)
        {
          number = "0" + number;
        }
        line += level.Grid[x, y] != null ? number : "..";
      }
      GD.Print(line);
    }
    
  }
  
  private static int GetRandomWeighted(List<(Vector2I position, int weight)> array, Level level, Vector2I levelCenter)
  {
    var count = array.Count;
    float totalWeight = 0;
    
    for (var i = 0; i < count; i++)
    {
      totalWeight += level.WeightFunction(array[i].position, levelCenter, array[i].weight);
    }
    
    var random = (float)GD.RandRange(0, totalWeight);

    for (var i = 0; i < count; i++)
    {
      random -= level.WeightFunction(array[i].position, levelCenter, array[i].weight);
      if (random <= 0)
      {
        return i;
      }
    }

    return array.Count-1;
  }

  private static List<Vector2I> GetEndRooms(ref int[,] weightGrid, Vector2I position)
  {
    var endRooms = new List<Vector2I>();
    
    var neighbours = new Array<Vector2I>
    {
      new Vector2I(0, -1),
      new Vector2I(1, 0),
      new Vector2I(0, 1),
      new Vector2I(-1, 0)
    };
    
    var hasNeighbour = false;
    
    foreach (var pos in neighbours)
    {
      var neighbour = position + pos;
      if (neighbour.X >= 0 && neighbour.X < weightGrid.GetLength(0) && neighbour.Y >= 0 && neighbour.Y < weightGrid.GetLength(1))
      {
        if (weightGrid[neighbour.X, neighbour.Y] == -1) continue;
        if (weightGrid[position.X, position.Y] > weightGrid[neighbour.X, neighbour.Y]) continue;
        
        endRooms.AddRange(GetEndRooms(ref weightGrid, neighbour));
        hasNeighbour = true;
      }
    }
    
    if (!hasNeighbour)
    {
      endRooms.Add(position);
    }

    return endRooms;
  }

  private static RoomExit GetRoomExits(ref int[,] weightGrid, Vector2I position)
  {
    var exits = new RoomExit();
    
    for (var i = 0; i < Neighbours.Count; i++)
    {
      var neighbour = position + Neighbours[i];
      if (neighbour.X >= 0 && neighbour.X < weightGrid.GetLength(0) && neighbour.Y >= 0 && neighbour.Y < weightGrid.GetLength(1))
      {
        if (weightGrid[position.X, position.Y] == weightGrid[neighbour.X, neighbour.Y]+1)
        {
          exits |= (RoomExit)(1 << i);
        }
      }
    }

    return exits;
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