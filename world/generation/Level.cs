using Flamme.entities.staff;
using Flamme.world.rooms;
using Godot;

namespace Flamme.world.generation;

public partial class Level : Node2D
{
  [Export] public Room Spawn;
  [Export] public PlayableCharacter PlayableCharacter;
  [Export] public Staff ActiveStaff;
  [Export] public PlayerCamera PlayerCamera;

  public override void _Ready()
  {
    LevelManager.Instance.SetLevelActive(this);
  }

  // TODO: Add grid and everything about a level here
}