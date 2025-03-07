using Flamme.world;
using Godot;
using System;

public partial class PlayerRoomLabel : Label
{
  private double TIMER_LIMIT = 0.3f;
  private double timer = 0.0;
  
  public override void _Process(double delta)
  {
    timer += delta;
    if (timer > TIMER_LIMIT)
    {
      timer = 0.0;
      if (LevelManager.Instance.CurrentLevel != null && LevelManager.Instance.CurrentLevel.PlayerCamera != null
          && LevelManager.Instance.CurrentLevel.PlayerCamera.GetActiveRoom() != null)
      {
        Text = LevelManager.Instance.CurrentLevel.PlayerCamera.GetActiveRoom().Name;
      }
    }
  }
}
