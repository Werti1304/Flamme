using Flamme.common.constant;
using Flamme.common.enums;
using System;
using Flamme.entities.staff;
using Flamme.testing;
using Flamme.ui;
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

  private List<(Vector2I v1, Room r1, Vector2I v2, Room r2)> _roomTransitions = [];

  public void FillRoomTransitionList()
  {
    for (var y = 0; y < Grid.GetLength(1); y++)
    {
      for (var x = 0; x < Grid.GetLength(0); x++)
      {
        var room = Grid[x, y];
        if(room == null)
          continue;
        
        foreach (var roomExit in Enum.GetValues<RoomExit>())
        {
          // Continue if there is no exit or door is already placed
          if (!room.ActualExits.HasFlag(roomExit) || room.Doors.ContainsKey(roomExit))
            continue;
        
          // Have to place doors
          var neighbour = GetRoomNeighbour(x, y, roomExit);
          var exitPosition = Dimensions.GetExitPosition(room.Size, roomExit);

          var door = SceneLoader.Instance[SceneLoader.Scene.Door].Instantiate<Door>();
          door.SetPerRoomTypes(room.Type, neighbour.Type);
          
          room.AddChild(door);
          
          var doorPos = exitPosition * 32;
          switch (roomExit)
          {
            case RoomExit.North:
            case RoomExit.North2:
            case RoomExit.North3:
            case RoomExit.South:
            case RoomExit.South2:
            case RoomExit.South3:
              doorPos.X += 16;
              doorPos.Y += 32;
              break;
            case RoomExit.West:
            case RoomExit.West2:
            case RoomExit.West3:
            case RoomExit.East:
            case RoomExit.East2:
            case RoomExit.East3:
              doorPos.X += 32;
              doorPos.Y += 16;
              door.RotationDegrees = 90;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
          door.Position = doorPos;
          
          GD.Print($"Placed door from {room.Name} to {neighbour.Name} at {doorPos}");
          
          room.Doors[roomExit] = door;
          neighbour.Doors[Dimensions.OppositeExits[roomExit]] = door;
        }
      }
    }
  }

  private Room GetRoomNeighbour(int startX, int startY, RoomExit roomExit)
  {
    int gridX;
    int gridY;
    
    switch (roomExit)
    {
      case RoomExit.North:
        gridX = 0;
        gridY = -1;
        break;
      case RoomExit.South:
        gridX = 0;
        gridY = 1;
        break;
      case RoomExit.West:
        gridX = -1;
        gridY = 0;
        break;
      case RoomExit.East:
        gridX = 1;
        gridY = 0;
        break;
      // TODO
      case RoomExit.North2:
      case RoomExit.South2:
      case RoomExit.West2:
      case RoomExit.East2:
      case RoomExit.North3:
      case RoomExit.South3:
      case RoomExit.West3:
      case RoomExit.East3:
      default:
        throw new ArgumentOutOfRangeException(nameof(roomExit), roomExit, null);
    }
    
    gridX += startX;
    gridY += startY;
    
    if(gridX >= 0 && gridX < Grid.GetLength(0) && gridY >= 0 && gridY < Grid.GetLength(1))
      return Grid[gridX, gridY];
    GD.PushError($"Tried to get room neighbour of {startX}, {startY} with exit {roomExit} but got out of bounds!");
    return null;
  }

  public override void _Ready()
  {
    WorldGenerator.Instance.OnLevelReady(level: this);
    LevelManager.Instance.CurrentLevel = this;

    if (Hud.Instance.Visible == false)
    {
      var tween = GetTree().CreateTween();
      
      Hud.Instance.MainContainer.Modulate = Colors.Transparent;
      Hud.Instance.Vignette.Modulate = Colors.Transparent;
      Hud.Instance.Show();
      tween.TweenProperty(Hud.Instance.MainContainer, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 3.0f);
      tween.Parallel().TweenProperty(Hud.Instance.Vignette, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 3.0f);
    }
  }

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