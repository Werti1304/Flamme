using Flamme.world;
using Godot;

namespace Flamme.ui;

public partial class PlayerPosLabel : Label
{
  private double TIMER_LIMIT = 0.3f;
  private double timer = 0.0;
  
  public override void _Process(double delta)
  {
    timer += delta;

    if (!(timer > TIMER_LIMIT))
      return;

    timer = 0.0;
    if(LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
      return;
    
    var player = LevelManager.Instance.CurrentLevel.PlayableCharacter;
    if (player == null || !IsInstanceValid(player))
      return;
    
    var pos = LevelManager.Instance.CurrentLevel.PlayableCharacter.GlobalPosition;
    Text = $"{pos.X:0.00}, {pos.Y:0.00}";
  }
}