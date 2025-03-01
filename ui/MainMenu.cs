using Flamme.world.generation;
using Godot;
using System;

public partial class MainMenu : Control
{
  public override void _Ready()
  {
    WorldGenerator.Instance.GenerateLevels();
  }
}
