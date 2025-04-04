using Flamme.world;
using Godot;

namespace Flamme.ui;

public partial class PlayerRoomLabel : Label
{
  private double TIMER_LIMIT = 0.3f;
  private double timer;
  
  public override void _Process(double delta)
  {
    timer += delta;
    if (timer > TIMER_LIMIT)
    {
      timer = 0.0;
      if (LevelManager.Instance.CurrentLevel != null && LevelManager.Instance.CurrentLevel.PlayerCamera != null
                                                     && LevelManager.Instance.CurrentLevel.PlayerCamera.GetActiveRoom() != null
                                                     && IsInstanceValid(LevelManager.Instance.CurrentLevel.PlayerCamera.GetActiveRoom()))
      {
        Text = LevelManager.Instance.CurrentLevel.PlayerCamera.GetActiveRoom().Name;
      }
    }
  }
}