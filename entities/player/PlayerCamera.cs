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

    if (room.CameraFixed)
    {
      LimitLeft = -10000000;
      LimitTop = -10000000;
      LimitRight = 10000000;
      LimitBottom = 10000000;
      GlobalPosition = room.MidPoint.GlobalPosition;
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
    if (_roomChange)
    {
      _roomChange = false;
      GlobalPosition = Player.GlobalPosition;
    }
    if (SmoothingEnabled)
    {
      GlobalPosition = GlobalPosition.Lerp(Player.GlobalPosition, _weight);
    }
    else
    {
      GlobalPosition = Player.GlobalPosition;
    }
  }
}
