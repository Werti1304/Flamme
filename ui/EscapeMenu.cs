using Flamme.common.input;
using Flamme.common.scenes;
using Flamme.world;
using Flamme.world.generation;
using Flamme.world.rooms;
using Godot;

namespace Flamme.ui;

public partial class EscapeMenu : CanvasLayer
{
  [Export] public StatsDisplay StatsDisplay;
  [Export] private Control _focusStartElement;
  
  public override void _Ready()
  {
    Hide();
  }

  public override void _Input(InputEvent @event)
  {
    if (!IsInstanceValid(Level.Current))
      return;
    
    if (@event.IsActionPressed(PlayerInputMap.Dict[PlayerInputMap.Action.Pause]))
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
    _focusStartElement.GrabFocus();
    Show();
  }
  
  private void Resume()
  {
    //Engine.TimeScale = 0;
    GetTree().Paused = false;
    Hide();
    Hud.Instance.PurseDisplay.Modulate = Colors.White;
  }

  public void OnMainMenuButtonPressed()
  {
    GetTree().Paused = false;
    Hide();
    Hud.Instance.PurseDisplay.Modulate = Colors.White;
    Hud.Instance.Hide();
    Tutorial.Reset();
    Main.Instance.UnloadingLevel = true;
    GetTree().ChangeSceneToPacked(SceneLoader.Instance[SceneLoader.Scene.MainMenu]);
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