using Flamme.common.constant;
using Flamme.common.enums;
using System;
using Flamme.entities.staff;
using Flamme.testing;
using Flamme.ui;
using Flamme.world.doors;
using Godot;
using System.Collections.Generic;
using Godot.Collections;
using System.Diagnostics;
using Room = Flamme.world.rooms.Room;

namespace Flamme.world.generation;

public partial class Level : Node2D
{
  [Export] public Room Spawn;
  [Export] public PlayableCharacter PlayableCharacter;
  [Export] public Staff ActiveStaff;
  [Export] public PlayerCamera PlayerCamera;

  public Room[,] Grid = new Room[16, 16];
  public static Level Current => LevelManager.Instance.CurrentLevel;

  [ExportGroup("Meta")] 
  [Export] public Node2D LootParent;
  [Export] public Node2D DoorParent;

  private List<(Vector2I v1, Room r1, Vector2I v2, Room r2)> _roomTransitions = [];

  public void Teleport(Room fromRoom, Room toRoom, Vector2 newPlayerPosition)
  {
    GD.Print($"Teleporting from {fromRoom.Name} to {toRoom.Name}");
    if (fromRoom != Room.Current)
    {
      GD.PushWarning("Tried to teleport from room that is not current room!");
    }
    
    fromRoom.LeaveRoom();
    toRoom.EnterRoom(PlayableCharacter);
    
    var positionDiff = newPlayerPosition - PlayableCharacter.GlobalPosition;
    GD.Print($"Position diff: {positionDiff}");
    PlayableCharacter.SetDeferred(Node2D.PropertyName.GlobalPosition, newPlayerPosition);
    
    // Teleport staff to new room but don't change relative distance to player 
    ActiveStaff?.TeleportNextFrame(ActiveStaff.GlobalPosition + positionDiff);
  }

  public void AddRoom(Room room, int x, int y)
  {
    room.Name = $"{room.Name} {x}, {y}";
    Grid[x, y] = room;
    AddChild(room);
    room.Owner = this;
  }
  
  public void FillRoomTransitionList()
  {
    for (var y = 0; y < Grid.GetLength(1); y++)
    {
      for (var x = 0; x < Grid.GetLength(0); x++)
      {
        var room = Grid[x, y];
        if(room == null)
          continue;
        
        foreach (var roomExit in room.TheoreticalDoorMarkers)
        {
          var neighbour = GetRoomNeighbour(x, y, roomExit.Key);

          if (neighbour == null)
          {
            // If there's no neighbour in that direction, hide
            roomExit.Value.Disguise();
            continue;
          }
          
          var neighbourDoorDirection = roomExit.Key.Opposite();
          if (!neighbour.TheoreticalDoorMarkers.ContainsKey(neighbourDoorDirection))
          {
            GD.PushError($"Room {neighbour.Name} has no door marker for {neighbourDoorDirection}, can't place down door!");
            continue;
          }
          
          var door = SceneLoader.Instance[SceneLoader.Scene.Door].Instantiate<Door>();
          
          // Setup door
          door.DoorMarker1 = roomExit.Value;
          door.Room1 = room;
          room.Doors[roomExit.Key] = door;
          
          door.DoorMarker2 = neighbour.TheoreticalDoorMarkers[neighbourDoorDirection];
          door.Room2 = neighbour;
          neighbour.Doors[neighbourDoorDirection] = door;
          neighbour.TheoreticalDoorMarkers.Remove(neighbourDoorDirection);
  
          door.SetPerRoomTypes(room.Type, neighbour.Type);
          room.AddChild(door);
          
          GD.Print($"Placed door from {room.Name} {roomExit.Key} to {neighbour.Name} {neighbourDoorDirection}.");
        }
        // Delete everything from here so we can't do dumb stuff even if we try to
        room.TheoreticalDoorMarkers.Clear();
      }
    }
  }
  
  private Room GetRoomNeighbour(int startX, int startY, Cardinal direction)
  {
    var directionVector = direction.ToVector();
    
    var gridX = startX + directionVector.X;
    var gridY = startY + directionVector.Y;

    if (gridX < 0 || gridX >= Grid.GetLength(0) || gridY < 0 || gridY >= Grid.GetLength(1))
      return null;
    
    return Grid[gridX, gridY];
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