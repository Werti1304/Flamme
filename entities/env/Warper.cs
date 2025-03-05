using Flamme.world;
using Godot;

namespace Flamme.entities.env;

public partial class Warper : Area2D
{
  [Export] public PackedScene NewLevel;

  public override void _Ready()
  {
    BodyEntered += OnBodyEntered;
  }

  private void OnBodyEntered(Node2D body)
  {
    if (body is PlayableCharacter playableCharacter)
    {
      // Fade player out
      LevelManager.Instance.StartlevelChange(NewLevel);
    }
  }
}