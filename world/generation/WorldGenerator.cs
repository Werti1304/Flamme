using System;
using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.entities.env.Loot;
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
  private static readonly Array<Vector2I> Neighbours = new Array<Vector2I>
  { // needs same order as RoomExit
    new Vector2I(0, -1), // north
    new Vector2I(1, 0),  // east
    new Vector2I(0, 1),  // south
    new Vector2I(-1, 0), // west
  };

  public override void _Ready()
  {
    RoomMeta.Init();
  }

  public System.Collections.Generic.Dictionary<LevelFloor, Level> Levels = new System.Collections.Generic.Dictionary<LevelFloor, Level>();
  public bool WaitingForSceneChangeToNewLevel = false;
  public bool GenerateNewLevel = true;

  public void GenerateLevels(ulong seed)
  {
    GD.Seed(seed);
    var levelScene = GD.Load<PackedScene>(PathConstants.LevelScenePath);
    WaitingForSceneChangeToNewLevel = true;
    
    // Transition to level 0
    // GetTree().CallDeferred("change_scene_to_packed", levelScene);
    GetTree().ChangeSceneToPacked(levelScene);
  }

  public void OnLevelReady(Level level)
  {
    if(!GenerateNewLevel)
      return;
    GenerateNewLevel = true;
    LevelManager.Instance.CurrentLevel = level;
    
    Debug.Assert(WaitingForSceneChangeToNewLevel);
    Debug.Assert(GetTree().CurrentScene == level);
    WaitingForSceneChangeToNewLevel = false;

    // Generate next level
    var levelGenerated = false;
    foreach (var levelFloor in Enum.GetValues<LevelFloor>())
    {
      if(Levels.ContainsKey(levelFloor))
        continue;
      GenerateLevel(level, levelFloor);
      Levels[levelFloor] = level;
      levelGenerated = true;
      break;
    }

    if (!levelGenerated)
    {
      // We're on the last level
      var winRoom = GD.Load<PackedScene>(PathConstants.WinRoomPath).Instantiate<Room>();
      level.Spawn = winRoom;
      var levelSize = new Vector2I(level.Grid.GetLength(0), level.Grid.GetLength(1));
      var levelCenter = levelSize / 2;
      level.AddRoom(winRoom, levelCenter.X, levelCenter.Y);
    }
    
    LevelManager.Instance.SetLevelActive(level);
  }

  public void GenerateLevel(Level level, LevelFloor floor)
  {
    if (Levels.ContainsKey(floor))
    {
      GD.PushError($"Level {floor} already exists!");
      return;
    }
    
    var levelSize = new Vector2I(level.Grid.GetLength(0), level.Grid.GetLength(1));
    var levelCenter = levelSize / 2;

    GD.Print("Generating spawn...");
    // Good enough for now as theres only 1 spawn
    var spawnData = RoomMeta.RoomDict[RoomType.Spawn][0];
    var spawn = spawnData.RoomScene.Instantiate<Room>();
    level.AddChild(spawn);
    level.Spawn = spawn;
    level.Grid[levelCenter.X, levelCenter.Y] = spawn;
    GD.Print($"Placing down spawn {spawn.Name} at {spawn.GlobalPosition}");

    GD.Print("Generating rooms...");
    GenerateRooms(level, floor);
    GD.Print("Level generated!");
    
    GD.Print("Placing down doors and closing sides...");
    level.FillRoomTransitionList();
    GD.Print("Doors placed!");

    GD.Print("Generating loot...");

    foreach (var room in level.Grid)
    {
      if (room == null)
        continue;
      room.GenerateLoot();
    }
    GD.Print("Loot generated!");

    GD.Print("Level fully generated!");
  }

  private void GenerateRooms(Level level, LevelFloor levelFloor)
  {
    var roomCount = levelFloor switch
    {
      LevelFloor.Prison1 => 16,
      LevelFloor.Prison2 => 32,
      _ => throw new ArgumentOutOfRangeException(nameof(levelFloor), levelFloor, null)
    };
    
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
    while (potNeighbours.Count > 0 && currRoomCount < roomCount)
    {
      var room = PopRandomRoomWeighted(ref potNeighbours, level, levelCenter);
      
      weightGrid[room.position.X, room.position.Y] = room.weight;
      
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
        var newNeighbour = room.position + pos;
        if ((!level.IsPosValid(newNeighbour)) ||                // out of bounds
            (weightGrid[newNeighbour.X, newNeighbour.Y] != -1) ||     // already filled
            (level.CountNeighbours(weightGrid, newNeighbour) > 1)) // too many neighbours
        {
          continue;
        }
          
        potNeighbours.Add((newNeighbour, room.weight+1));
      }
      
      currRoomCount++;
    }
    
    // room types that can replace end rooms, except boss room
    var endRoomTypeCount = new List<(RoomType type, int count)>
    {
      (RoomType.Boss, 1),
      (RoomType.Treasure, 1),
      (RoomType.Shop, 1),
      (RoomType.Smithy, 1),
      (RoomType.Secret, 1),
    };
    
    // TODO: globals.cs for tile size and room size
    var tileSize = new Vector2I(32, 32);
    var roomSize = new Vector2I(64, 32);
    
    // end rooms
    var endRoomsConst = GetEndRooms(ref weightGrid, levelCenter);
    var endRooms = new List<Vector2I>(endRoomsConst);
    var randomEndRoomIndex = -1;
    var endRoomShuffled = Shuffle(endRooms.Count);
    
    while (randomEndRoomIndex < endRooms.Count-1 & endRoomTypeCount.Count > 0)
    {
      randomEndRoomIndex++;
      var typeCountShuffled = Shuffle(endRoomTypeCount.Count);
      var randomEndRoom = endRooms[endRoomShuffled[randomEndRoomIndex]];

      // try every room type for current end room
      for (int i = 0; i < endRoomTypeCount.Count; i++)
      {
        var roomTypeCount = endRoomTypeCount[typeCountShuffled[i]];
        var roomMeta = RoomMeta.GetRandomRoom(roomTypeCount.type, GetRoomExits(ref weightGrid, randomEndRoom), levelFloor);
        
        if (roomMeta == null) continue;
        
        endRoomTypeCount[typeCountShuffled[i]] = (roomTypeCount.type, roomTypeCount.count-1); // decrease count
        if (endRoomTypeCount[typeCountShuffled[i]].count == 0) // remove room from list if count is 0
        {
          endRoomTypeCount.RemoveAt(typeCountShuffled[i]);
        }
        
        weightGrid[randomEndRoom.X, randomEndRoom.Y] = -(int)roomTypeCount.type; // change weight to room type for debugging
        
        var room = roomMeta.RoomScene.Instantiate<Room>();
        // TODO room.ActualExits = GetRoomExits(ref weightGrid, randomEndRoom);
        var placeGlobalPos = (randomEndRoom - levelCenter) * roomSize * tileSize;
        GD.Print($"Placing down room {room.Name} at {placeGlobalPos}, index {randomEndRoom}");
        room.GlobalPosition = placeGlobalPos;
        level.AddRoom(room, randomEndRoom.X, randomEndRoom.Y);
        
        break;
      }
    }
    
    // load paths and spawn
    for (var y = 0; y < level.Grid.GetLength(1); y++)
    {
      for (var x = 0; x < level.Grid.GetLength(0); x++)
      {
        if (level.Grid[x, y] != null || weightGrid[x, y] <= -1) 
          continue;
        
        if (y == levelCenter.Y && x == levelCenter.X)
          continue;
        
        var roomPos = new Vector2I(x, y);
        var actualExits = GetRoomExits(ref weightGrid, roomPos);
        var roomMeta = RoomMeta.GetRandomRoom(RoomType.Pathway, actualExits, levelFloor);
        var room = roomMeta.RoomScene.Instantiate<Room>();
        var placeGlobalPos = (roomPos - levelCenter) * roomSize * tileSize;
        GD.Print($"Placing down room {room.Name} at {placeGlobalPos}");
        room.GlobalPosition = placeGlobalPos;
        level.AddRoom(room, x, y);
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

  private List<int> Shuffle(int length)
  {
    var list = new List<int>();
    for (var i = 0; i < length; i++)
    {
      list.Add(i);
    }
    
    for (var i = 0; i < length; i++)
    {
      var randomIndex = GD.RandRange(0, length-1);
      (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
    }
    
    return list;
  }
  
  private (Vector2I position, int weight) PopRandomRoomWeighted(ref List<(Vector2I position, int weight)> array, Level level, Vector2I levelCenter)
  {
    var count = array.Count;
    var index = count-1;
    var totalWeight = 0.0f;
    
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
        index = i;
        break;
      }
    }
    
    var room = array[index];
    array.RemoveAt(index);

    return room;
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
    
    var hasNeighbour = false;
    
    foreach (var pos in Neighbours)
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

  private static List<Cardinal> GetRoomExits(ref int[,] weightGrid, Vector2I position)
  {
    var exits = new List<Cardinal>();
    
    for (var i = 0; i < Neighbours.Count; i++)
    {
      var neighbour = position + Neighbours[i];
      if (neighbour.X >= 0 && neighbour.X < weightGrid.GetLength(0) && neighbour.Y >= 0 && neighbour.Y < weightGrid.GetLength(1))
      {
        if (weightGrid[neighbour.X, neighbour.Y] != -1)
        {
          // TODO Wtf she doin
          exits.Add((Cardinal)i);
          // exits |= (RoomExit)(1 << i);
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