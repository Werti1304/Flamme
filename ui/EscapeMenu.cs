using Flamme.testing;
using Godot;

namespace Flamme.ui;

public partial class EscapeMenu : CanvasLayer
{
  public override void _Ready()
  {
    Hide();
  }

  public override void _Input(InputEvent @event)
  {
    if (@event.IsActionPressed(Const.InputMap.EscapeMenu))
    {
      if (GetTree().Paused)
      {
        Resume();
      }
      else
      {
        Pause();
      }   
      GetViewport().SetInputAsHandled();
    }
  }

  private void Pause()
  {
    //Engine.TimeScale = 0;
    GetTree().Paused = true;
    Show();
  }
  
  private void Resume()
  {
    //Engine.TimeScale = 0;
    GetTree().Paused = false;
    Hide();
  }

  private void Quit()
  {
    GetTree().Quit();
  }
}