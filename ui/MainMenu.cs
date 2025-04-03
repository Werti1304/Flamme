using Flamme.common.constant;
using Flamme.world.generation;
using Godot;

namespace Flamme.ui;

public partial class MainMenu : CanvasLayer
{
  [ExportGroup("Meta")] 
  [Export] private SeedSelector _spinBox;

  [Export] private Control _focusStartElement;

  public override void _Ready()
  {
    Main.Instance.UnloadingLevel = false;
    _focusStartElement.CallDeferred(Control.MethodName.GrabFocus);
  }

  public void OnNewGame()
  {
    Main.Instance.StartNewGame((ulong)_spinBox.Value);
  }

  public void OnDev()
  {
    WorldGenerator.Instance.GenerateNewLevel = false;
    var levelScene = GD.Load<PackedScene>(PathConstants.ExampleLevelPath);
    GetTree().CallDeferred("change_scene_to_packed", levelScene);
  }

  public void OnQuit()
  {
    Main.Instance.Shutdown();
  }
}