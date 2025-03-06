using Flamme.common.constant;
using Godot;
using Godot.Collections;
using System;

public partial class SceneLoader : Node2D
{
  public enum Scene
  {
    Level,
    Warper,
    Player,        // Todo AP (after prototype) Replace with randomized player char
    PlayerCamera,
    StartingStaff, // Todo AP Replace with randomized staff
    ItemPickup,
    PursePickup,
    HealthPickup,
    Chest, // TODO 3
  }

  public readonly Dictionary<Scene, PackedScene> Scenes = new Dictionary<Scene, PackedScene>()
  {
    { Scene.Level, GD.Load<PackedScene>(PathConstants.LevelScenePath) },
    { Scene.Warper, GD.Load<PackedScene>(PathConstants.WarperScenePath) },
    { Scene.Player, GD.Load<PackedScene>(PathConstants.PlayerScenePath) },
    { Scene.PlayerCamera, GD.Load<PackedScene>(PathConstants.PlayerCameraScenePath) },
    { Scene.StartingStaff, GD.Load<PackedScene>(PathConstants.StartingStaffScenePath) },
    { Scene.ItemPickup, GD.Load<PackedScene>(PathConstants.ItemPickupScenePath) },
    { Scene.PursePickup, GD.Load<PackedScene>(PathConstants.PursePickupScenePath) },
    { Scene.HealthPickup, GD.Load<PackedScene>(PathConstants.HealthPickupScenePath) },
    // { Scene.Chest, GD.Load<PackedScene>(PathConstants.ChestScenePath) },

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
