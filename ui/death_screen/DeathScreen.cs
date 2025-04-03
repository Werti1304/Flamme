using Flamme.common.input;
using Flamme.common.scenes;
using Flamme.world.rooms;
using Godot;

namespace Flamme.ui.death_screen;

public partial class DeathScreen : CanvasLayer
{
  [Export] private Control _focusStartElement;
  
  public override void _Ready()
  {
    Hide();
    SetProcessInput(false);
  }

  public void ShowDeathScreen()
  {
    Show();
    _focusStartElement.GrabFocus();
    SetProcessInput(true);
  }

  public override void _Input(InputEvent @event)
  {
    // Disable all gameplay keys
    foreach (var inputs in PlayerInputMap.Dict)
    {
      if (@event.IsAction(inputs.Value))
      {
        GetViewport().SetInputAsHandled();
      }
    }
  }

  public void OnMainMenuButtonPressed()
  {
    Hide();
    Hud.Instance.Hide();
    SetProcessInput(false);
    Tutorial.Reset();
    Main.Instance.UnloadingLevel = true;
    GetTree().ChangeSceneToPacked(SceneLoader.Instance[SceneLoader.Scene.MainMenu]);
  }
  
  private static DeathScreen _instance;
  private static readonly object Padlock = new object();
  
  public DeathScreen()
  {
    _instance = this;
  }
  
  public static DeathScreen Instance
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