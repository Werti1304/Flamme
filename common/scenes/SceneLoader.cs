using Flamme.common.constant;
using Godot;
using Godot.Collections;

namespace Flamme.common.scenes;

public partial class SceneLoader : Node2D
{
  public enum Scene
  {
    MainMenu,
    Level,
    Warper,
    Player,        // Todo AP (after prototype) Replace with randomized player char
    PlayerCamera,
    StartingStaff, // Todo AP Replace with randomized staff
    ItemPickup,
    PursePickup,
    HealthPickup,
    Chest, // TODO 3
    Runner,
    RunnerSmart,
    FireflyNeutral,
    FireflyHoming,
    FireflyRaging,
    Door,
    DoorMarker,
    Trailing,
    Fireball,
    Blargh
  }

  public readonly Dictionary<Scene, PackedScene> Scenes = new Dictionary<Scene, PackedScene>()
  {
    { Scene.MainMenu, GD.Load<PackedScene>(PathConstants.MainMenuScenePath) },
    { Scene.Level, GD.Load<PackedScene>(PathConstants.LevelScenePath) },
    { Scene.Warper, GD.Load<PackedScene>(PathConstants.WarperScenePath) },
    { Scene.Player, GD.Load<PackedScene>(PathConstants.PlayerScenePath) },
    { Scene.PlayerCamera, GD.Load<PackedScene>(PathConstants.PlayerCameraScenePath) },
    { Scene.StartingStaff, GD.Load<PackedScene>(PathConstants.StartingStaffScenePath) },
    { Scene.ItemPickup, GD.Load<PackedScene>(PathConstants.ItemPickupScenePath) },
    { Scene.PursePickup, GD.Load<PackedScene>(PathConstants.PursePickupScenePath) },
    { Scene.HealthPickup, GD.Load<PackedScene>(PathConstants.HealthPickupScenePath) },
    { Scene.Chest, GD.Load<PackedScene>(PathConstants.ChestScenePath) },
    { Scene.Runner, GD.Load<PackedScene>(PathConstants.RunnerScenePath) },
    { Scene.RunnerSmart, GD.Load<PackedScene>(PathConstants.RunnerSmartScenePath) },
    { Scene.FireflyNeutral, GD.Load<PackedScene>(PathConstants.FireflyNeutralScenePath) },
    { Scene.FireflyHoming, GD.Load<PackedScene>(PathConstants.FireflyHomingScenePath) },
    { Scene.FireflyRaging, GD.Load<PackedScene>(PathConstants.FireflyRagingScenePath) },
    { Scene.Door, GD.Load<PackedScene>(PathConstants.DoorScenePath) },
    { Scene.DoorMarker, GD.Load<PackedScene>(PathConstants.DoorMarkerScenePath) },
    { Scene.Trailing, GD.Load<PackedScene>(PathConstants.TrailingProjectileScenePath) },
    { Scene.Fireball, GD.Load<PackedScene>(PathConstants.FireballProjectileScenePath) },
    { Scene.Blargh, GD.Load<PackedScene>(PathConstants.BlarghProjectileScenePath) },
  };
  
  public PackedScene this[Scene scene] => Scenes[scene]; 
  
  private static SceneLoader _instance;
  private static readonly object Padlock = new();
  
  public SceneLoader()
  {
    _instance = this;
  }
  
  public static SceneLoader Instance
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