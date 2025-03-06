using Flamme.world;
using Godot;

namespace Flamme.entities.env;

public partial class Warper : Area2D
{
  [Export] public PackedScene NewLevel;

  public override void _Ready()
  {
    Modulate = Colors.Transparent;

    var tween = GetTree().CreateTween();
    tween.TweenProperty(this, "modulate", Colors.White, 3.0f).SetTrans(Tween.TransitionType.Sine);
    tween.TweenCallback(new Callable(this, nameof(Enable)));
  }

  private void Enable()
  {
    BodyEntered += OnBodyEntered;

    foreach (var body in GetOverlappingBodies())
    {
      OnBodyEntered(body);
    }
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