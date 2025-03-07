using System;
using Flamme.entities.staff;
using Flamme.testing;
using Godot;
using System.Collections.Generic;
using Godot.Collections;
using Room = Flamme.world.rooms.Room;

namespace Flamme.world.generation;

public partial class Level : Node2D
{
  [Export] public Room Spawn;
  [Export] public PlayableCharacter PlayableCharacter;
  [Export] public Staff ActiveStaff;
  [Export] public PlayerCamera PlayerCamera;

  public Room[,] Grid = new Room[16, 16];

  [ExportGroup("Meta")] 
  [Export] public Node2D LootParent;

  public override void _Ready()
  {
    if (WorldGenerator.Instance.GeneratingFirstLevel)
    {
      WorldGenerator.Instance.GenerateFirstLevel(this);
      WorldGenerator.Instance.GeneratingFirstLevel = false;
    }
    LevelManager.Instance.SetLevelActive(this);
  }

  // TODO: Add grid and everything about a level here
  // Do all of the generation part in WorldGenerator
  
  public bool IsPosValid(Vector2 position)
  {
    return position.X >= 0 && position.X < Grid.GetLength(0) && position.Y >= 0 && position.Y < Grid.GetLength(1);
  }
  
  private static int MinkowskiDistance(Vector2I a, Vector2I b)
  {
    return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
  }
  
  public float WeightFunction(Vector2I position, Vector2I levelCenter, int weight)
  {
    float distance =  16.0f / Mathf.Max(MinkowskiDistance(position, levelCenter), 0.01f);
    return weight * Mathf.Pow(distance, 3); 
  }
  
  public int CountNeighbours(Vector2I position)
  {
    Array<Vector2I> neighbours = new()
    {
      new Vector2I(0, -1),
      new Vector2I(1, 0),
      new Vector2I(0, 1),
      new Vector2I(-1, 0)
    };
    
    int count = 0;
    foreach (var n in neighbours)
    {
      var pos = position + n;
      if (pos is { X: 0, Y: 0 }) continue;
      if (!IsPosValid(pos)) continue;
      if (Grid[pos.X, pos.Y] != null)
      {
        count++;
      }
    }
    return count;
  }
  
  public int CountNeighbours(int [,] weightGrid, Vector2I position)
  {
    Array<Vector2I> neighbours = new()
    {
      new Vector2I(0, -1),
      new Vector2I(1, 0),
      new Vector2I(0, 1),
      new Vector2I(-1, 0)
    };
    
    int count = 0;
    foreach (var n in neighbours)
    {
      var pos = position + n;
      if (pos is { X: 0, Y: 0 }) continue;
      if (!IsPosValid(pos)) continue;
      if (weightGrid[pos.X, pos.Y] != -1)
      {
        count++;
      }
    }
    return count;
  }
}