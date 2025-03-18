using Flamme.world;
using Flamme.world.generation;
using Flamme.world.rooms;
using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
  // Thanks to https://youtu.be/RbE7BKbo6_Q?si=o--rn2pyzzLWkozB
  [Export] public PlayableCharacter Player;
  [Export] public bool SmoothingEnabled = true;
  [Export(PropertyHint.Range, "0,10,")] public int SmoothingDistance = 8;

  private float _weight;
  private Room _activeRoom;
  
  public Room GetActiveRoom()
  {
    return _activeRoom;
  }

  private bool _roomChange = false;
  public void UpdateRoom()
  {
    var room = Room.Current;
    var roomRect = room.CollisionShape.Shape.GetRect();
    LimitLeft = (int)room.GlobalPosition.X;
    LimitTop = (int)room.GlobalPosition.Y;
    LimitRight = LimitLeft + (int)roomRect.Size.X;
    LimitBottom = LimitTop + (int)roomRect.Size.Y;
    _activeRoom = room;
    _roomChange = true;

    if (room.CameraFixedX)
    {
      LimitLeft = -10000000;
      LimitRight = 10000000;
    }

    if (room.CameraFixedY)
    {
      LimitTop = -10000000;
      LimitBottom = 10000000;
    }
    if (room.CameraFixedX && room.CameraFixedY)
    {
      GlobalPosition = _activeRoom.MidPoint.GlobalPosition;
      SetProcess(false);
    }
    else
    {
      SetProcess(true);
    }
  }

  public override void _Ready()
  {
    // 11 is one more than the max distance
    _weight = (11 - SmoothingDistance) / 100.0f;
  }

  public override void _Process(double delta)
  {
    if (Player == null || !IsInstanceValid(Player))
    {
      GD.PrintErr("PlayerCamera: Player is null, stopping camera");
      SetProcess(false);
      return;
    }
    if (_roomChange)
    {
      if (_activeRoom.CameraFixedX)
      {
        GlobalPosition = new Vector2(_activeRoom.MidPoint.GlobalPosition.X, Player.GlobalPosition.Y);
      }
      else if (_activeRoom.CameraFixedY)
      {
        GlobalPosition = new Vector2(Player.GlobalPosition.X, _activeRoom.MidPoint.GlobalPosition.Y);
      }
      else
      {
        GlobalPosition = Player.GlobalPosition;
      }
      _roomChange = false;
    }
    if (SmoothingEnabled)
    {
      if (_activeRoom.CameraFixedX)
      {
        GlobalPosition = GlobalPosition with { Y = Mathf.Lerp(GlobalPosition.Y, Player.GlobalPosition.Y, _weight) };
      }
      else if (_activeRoom.CameraFixedY)
      {
        GlobalPosition = GlobalPosition with { X = Mathf.Lerp(GlobalPosition.X, Player.GlobalPosition.X, _weight) };
      }
      else
      {
        GlobalPosition = GlobalPosition.Lerp(Player.GlobalPosition, _weight);
      }
    }
    else
    {
      GlobalPosition = Player.GlobalPosition;
    }
  }
}
