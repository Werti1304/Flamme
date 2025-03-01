using Flamme.common.enums;
using Flamme.world.rooms;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot.Collections;
using Array = System.Array;

namespace Flamme.world.generation;

public partial class WorldGenerator : Node2D
{
  // TODO: Add actual generation logic here
  public RandomNumberGenerator NotSeedRng = new RandomNumberGenerator();

  public const string RoomFolderPath = "world/rooms/";
  public System.Collections.Generic.Dictionary<RoomType, List<RoomData>> RoomDict = new System.Collections.Generic.Dictionary<RoomType, List<RoomData>>();

  private static readonly Array<Vector2I> Neighbours = new()
  { // needs same order as RoomExit
    new Vector2I(0, -1), // north
    new Vector2I(0, 1),  // south
    new Vector2I(-1, 0), // west
    new Vector2I(1, 0),  // east
  };

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

  public System.Collections.Generic.Dictionary<LevelFloor, Level> Levels = new System.Collections.Generic.Dictionary<LevelFloor, Level>();
  public const string LevelScenePath = "world/generation/Level.tscn";

  public bool GeneratingFirstLevel = true;

  public void GenerateLevels()
  {
    var levelScene = GD.Load<PackedScene>(LevelScenePath);
    
    // Transition to level 0
    // TODO Do this after pressing button in main menu instead of Main.cs
    GetTree().CallDeferred("change_scene_to_packed", levelScene);
  }

  private int getRandomWeighted(List<(Vector2I position, int weight)> array, Level level, Vector2I levelCenter)
  {
    int count = array.Count;
    float totalWeight = 0;
    
    for (int i = 0; i < count; i++)
    {
      totalWeight += level.WeightFunction(array[i].position, levelCenter, array[i].weight);
    }
    
    float random = (float)GD.RandRange(0, totalWeight);

    for (int i = 0; i < count; i++)
    {
      random -= level.WeightFunction(array[i].position, levelCenter, array[i].weight);
      if (random <= 0)
      {
        return i;
      }
    }

    return array.Count-1;
  }

  private List<Vector2I> getEndRooms(ref int[,] weightGrid, Vector2I position)
  {
    List<Vector2I> endRooms = new();
    
    Array<Vector2I> neighbours = new()
    {
      new Vector2I(0, -1),
      new Vector2I(1, 0),
      new Vector2I(0, 1),
      new Vector2I(-1, 0)
    };
    
    bool hasNeighbour = false;
    
    foreach (var pos in neighbours)
    {
      Vector2I neighbour = position + pos;
      if (neighbour.X >= 0 && neighbour.X < weightGrid.GetLength(0) && neighbour.Y >= 0 && neighbour.Y < weightGrid.GetLength(1))
      {
        if (weightGrid[neighbour.X, neighbour.Y] == -1) continue;
        if (weightGrid[position.X, position.Y] > weightGrid[neighbour.X, neighbour.Y]) continue;
        
        endRooms.AddRange(getEndRooms(ref weightGrid, neighbour));
        hasNeighbour = true;
      }
    }
    
    if (!hasNeighbour)
    {
      endRooms.Add(position);
    }

    return endRooms;
  }

  private RoomExit getRoomExits(ref int[,] weightGrid, Vector2I position)
  {
    RoomExit exits = new();
    
    for (int i = 0; i < Neighbours.Count; i++)
    {
      Vector2I neighbour = position + Neighbours[i];
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
  
  public void GenerateFirstLevel(Level level)
  {
    const int roomCount = 32;
    Vector2I levelSize = new(level.Grid.GetLength(0), level.Grid.GetLength(1));
    Vector2I levelCenter = levelSize / 2;
    
    // Good enough for now as theres only 1 spawn
    var spawnData = RoomDict[RoomType.Spawn][0];

    Debug.Assert(level == GetTree().CurrentScene);
    var spawn = spawnData.RoomScene.Instantiate<Room>();
    level.AddChild(spawn);
    level.Grid[levelCenter.X, levelCenter.Y] = spawn;
    
    GD.Print("Generating level...");
    
    int [,] weightGrid = new int[levelSize.X, levelSize.Y];
    // init weightGrid with -1
    for (int y = 0; y < levelSize.Y; y++)
    {
      for (int x = 0; x < levelSize.X; x++)
      {
        weightGrid[x, y] = -1;
      }
    }
    
    List<(Vector2I position, int weight)> potNeighbours = new(); // List of potential neighbours
    int currRoomCount = 0;
    
    potNeighbours.Add((levelCenter, 0));  // add spawn to queue
    
    // generate paths
    while (potNeighbours.Count > 0)
    {
      int index = getRandomWeighted(potNeighbours, level, levelCenter);
      Vector2I position = potNeighbours[index].position;
      int weight = potNeighbours[index].weight;
      
      // remove from queue
      potNeighbours.RemoveAt(index);
      level.Grid[position.X, position.Y] = new Room();
      weightGrid[position.X, position.Y] = weight;
      currRoomCount++;
      if (currRoomCount == roomCount) break;
      
      // remove invalid neighbours
      for (int i = 0; i < potNeighbours.Count; i++)
      {
        if ((weightGrid[potNeighbours[i].position.X, potNeighbours[i].position.Y] != -1) || // already filled
            (level.CountNeighbours(potNeighbours[i].position) > 1))                         // too many neighbours
        {
          potNeighbours.RemoveAt(i);
          i--;
        }
      }
      
      // add neighbours to queue
      foreach (var pos in Neighbours)
      {
        Vector2I neighbour = position + pos;
        if ((!level.IsPosValid(neighbour)) ||            // out of bounds
            (weightGrid[neighbour.X, neighbour.Y] != -1) || // already filled
            (level.CountNeighbours(neighbour) > 1))         // too many neighbours
        {
          continue;
        }
          
        potNeighbours.Add((neighbour, weight+1));
      }
    }
    
    // room types that can replace end rooms, except boss room
    Array<RoomType> endRoomTypes = new()
    {
      RoomType.Treasure,
      RoomType.Shop,
      RoomType.Smithy,
      //RoomType.Secret,
    };
    
    var tileSize = new Vector2I(16, 16);
    
    // end rooms
    bool hasBossRoom = false;
    var endRooms = getEndRooms(ref weightGrid, levelCenter);
    int rounds = 100; // pfusch
    while (endRooms.Count > 0 && rounds-- > 0)
    {
      int randomEndRoomIndex = GD.RandRange(0, endRooms.Count-1);
      
      //get random element from RoomDict
      var roomType = hasBossRoom ? endRoomTypes[GD.RandRange(0, endRoomTypes.Count-1)] : RoomType.Boss; // choose boss room at first
      if (RoomDict[roomType].Count == 0)  // if no room of this type exists remove it from list
      {
        endRoomTypes.Remove(roomType); //remove and try again
        continue;
      }
      var roomData = RoomDict[roomType][GD.RandRange(0, RoomDict[roomType].Count-1)]; // get random room of this type
      
      var exits = getRoomExits(ref weightGrid, endRooms[randomEndRoomIndex]); // get exits for room
      RoomExit actualExits = (RoomExit)((int)exits & (int)roomData.AllowedExits);
      if (actualExits == 0) continue;
      if (roomType == RoomType.Boss) hasBossRoom = true;
      
      weightGrid[endRooms[randomEndRoomIndex].X, endRooms[randomEndRoomIndex].Y] = -(int)roomType; // change weight to room type for debugging
      
      var room = roomData.RoomScene.Instantiate<Room>();
      room.ActualExits = actualExits;
      
      room.GlobalPosition = (new Vector2I(endRooms[randomEndRoomIndex].X * 17, endRooms[randomEndRoomIndex].Y * 11)-levelCenter) * tileSize;
      level.AddChild(room);
      level.Grid[endRooms[randomEndRoomIndex].X, endRooms[randomEndRoomIndex].Y] = room;
    }
    
    //load paths and spawn
    for (int y = 0; y < level.Grid.GetLength(1); y++)
    {
      for (int x = 0; x < level.Grid.GetLength(0); x++)
      {
        if (level.Grid[x, y] == null && weightGrid[x, y] != -1)
        {
          var roomData = RoomDict[RoomType.Pathway][0];
          if (y == levelCenter.Y && x == levelCenter.X)
          {
            continue;
          }
          var room = roomData.RoomScene.Instantiate<Room>();
          room.GlobalPosition = (new Vector2I(x * 17, y * 11)-levelCenter) * tileSize;
          level.AddChild(room);
          level.Grid[x, y] = room;
        }
      }
    }
    
    
    // print in console
    for (int y = 0; y < level.Grid.GetLength(1); y++)
    {
      string line = "";
      for (int x = 0; x < level.Grid.GetLength(0); x++)
      {
        string number = $"{weightGrid[x, y]}";
        // add leading 0s
        while (number.Length < 2)
        {
          number = "0" + number;
        }
        line += level.Grid[x, y] != null ? number : "--";
      }
      GD.Print(line);
    }
    
    GD.Print("Level generated!");
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