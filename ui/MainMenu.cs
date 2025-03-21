using Flamme;
using Flamme.common.constant;
using Flamme.world.generation;
using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
  [ExportGroup("Meta")] 
  [Export] private SpinBox _spinBox;
  
  public void OnNewGame()
  {
    WorldGenerator.Instance.GenerateLevels((ulong)_spinBox.Value);
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
