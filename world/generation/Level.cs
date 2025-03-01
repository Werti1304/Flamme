using Flamme.entities.staff;
using Flamme.world.rooms;
using Godot;
using System.Collections.Generic;

namespace Flamme.world.generation;

public partial class Level : Node2D
{
  [Export] public Room Spawn;
  [Export] public PlayableCharacter PlayableCharacter;
  [Export] public Staff ActiveStaff;
  [Export] public PlayerCamera PlayerCamera;

  public Room[,] Grid = new Room[16, 16];

  public override void _Ready()
  {
    LevelManager.Instance.SetLevelActive(this);

    WorldGenerator.Instance.ActualGeneration(this or GetTree().CurrentScene;);
    
  }

  // TODO: Add grid and everything about a level here
  // Do all of the generation part in WorldGenerator
}