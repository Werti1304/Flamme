using Flamme.testing;
using Godot;

namespace Flamme.ui;

public partial class EscapeMenu : CanvasLayer
{
  [Export] public StatsDisplay StatsDisplay;
  
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
    Hud.Instance.PurseDisplay.Modulate = Colors.Transparent;
    Show();
  }
  
  private void Resume()
  {
    //Engine.TimeScale = 0;
    GetTree().Paused = false;
    Hide();
    Hud.Instance.PurseDisplay.Modulate = Colors.White;
  }

  private void Quit()
  {
    Main.Instance.Shutdown();
  }
  
  private static EscapeMenu _instance;
  private static readonly object Padlock = new();
  
  public EscapeMenu()
  {
    _instance = this;
  }
  
  public static EscapeMenu Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
}