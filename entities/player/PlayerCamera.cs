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

  public void UpdateRoom()
  {
    var room = Room.Current;
    var roomRect = room.CollisionShape.Shape.GetRect();
    LimitLeft = (int)room.GlobalPosition.X;
    LimitTop = (int)room.GlobalPosition.Y;
    LimitRight = LimitLeft + (int)roomRect.Size.X;
    LimitBottom = LimitTop + (int)roomRect.Size.Y;
    _activeRoom = room;
  }

  public override void _Ready()
  {
    // 11 is one more than the max distance
    _weight = (11 - SmoothingDistance) / 100.0f;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (SmoothingEnabled)
    {
      GlobalPosition = GlobalPosition.Lerp(Player.GlobalPosition, _weight);
    }
    else
    {
      GlobalPosition = Player.GlobalPosition;
    }
    // GlobalPosition = GlobalPosition.Floor();
  }
}
