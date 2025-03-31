using Flamme.common.enums;
using Flamme.entities.staff;
using Flamme.ui;
using Flamme.world.generation;
using Flamme.world.rooms;
using Godot;
using System.Diagnostics;
using System.Linq;

namespace Flamme.world;

public partial class LevelManager : Node2D
{
  [Signal] public delegate void LevelReadyEventHandler();
  
  private Level _currentLevel;

  public Level CurrentLevel
  {
    get => _currentLevel;
    set
    {
      _currentLevel = value;
      if (value != null)
      {
        Hud.Instance.Minimap.UpdateLevel();
      }
    }
  }
  
  private struct TransferableStuff
  {
    public entities.player.PlayableCharacter Character;
    public Staff Staff;
    public entities.player.PlayerCamera Camera;

    public TransferableStuff(entities.player.PlayableCharacter character, Staff staff, entities.player.PlayerCamera camera)
    {
      Character = character;
      Staff = staff;
      Camera = camera;
    }
  }

  private TransferableStuff? _stuffToTransfer;

  public void Unload()
  {
    _currentLevel = null;

    if (_stuffToTransfer != null && _stuffToTransfer.HasValue)
    {
      if (_stuffToTransfer.Value.Staff != null)
      {
        _stuffToTransfer.Value.Staff.QueueFree();
        _stuffToTransfer.Value.Character.QueueFree();
        _stuffToTransfer.Value.Camera.QueueFree();
      }
      _stuffToTransfer = null;
    }
  }
  
  public void StartlevelChange(PackedScene levelScene)
  {
    Main.Instance.UnloadingLevel = true;
    var character = CurrentLevel.PlayableCharacter;
    character.SetPhysicsProcess(false);
    character.SetProcessInput(false);
    var shaderMaterial = (ShaderMaterial)Hud.Instance.Vignette.Material;
    var startingDarknessOut = (float)shaderMaterial.GetShaderParameter("darkness_out");
    var startingDarknessIn = (float)shaderMaterial.GetShaderParameter("darkness_in");
    var tween = GetTree().CreateTween();
    CreateShaderTween(tween, Hud.Instance.Vignette, "darkness_out", startingDarknessOut, 0.0f, 3.0f).SetTrans(Tween.TransitionType.Linear).Parallel();
    CreateShaderTween(tween, Hud.Instance.Vignette, "darkness_in", startingDarknessIn, 0.0f, 3.0f).SetTrans(Tween.TransitionType.Linear);
    tween.TweenCallback(Callable.From(() => ChangeLevel(levelScene)));
    CreateShaderTween(tween, Hud.Instance.Vignette, "darkness_in", 0.0f, startingDarknessIn, 3.0f).SetTrans(Tween.TransitionType.Linear).Parallel();
    CreateShaderTween(tween, Hud.Instance.Vignette, "darkness_out", 0.0f, startingDarknessOut, 3.0f).SetTrans(Tween.TransitionType.Linear);
    tween.TweenMethod(Callable.From<bool>((value) => character.SetPhysicsProcess(value)), false, true, 0.1f);
    tween.TweenMethod(Callable.From<bool>((value) => character.SetProcessInput(value)), false, true, 1.0f);
  }

  private void ChangeLevel(PackedScene levelScene)
  {
    // Stuff to bring into the new level
    var character = CurrentLevel.PlayableCharacter;
    character.GetParent().RemoveChild(character);
    
    var activeStaff = CurrentLevel.ActiveStaff;
    activeStaff?.GetParent().RemoveChild(activeStaff);

    var camera = CurrentLevel.PlayerCamera;
    camera.GetParent().RemoveChild(camera);
    
    _stuffToTransfer = new TransferableStuff(character, activeStaff, camera);
    // Change to new level
    // Not deferred by default, but should be, now we can't check error code :(
    // https://github.com/godotengine/godot/issues/85852
    GetTree().CallDeferred("change_scene_to_packed", levelScene);
    // Debug.Assert(GetTree().ChangeSceneToPacked(levelScene) == Error.Ok);
    // It will now need 1 frame to activate the new scene
  }
  
  public static Tween CreateShaderTween(Tween tween, Control node, string shaderProperty, float valueStart, float valueEnd, float duration)
  {
    // Ensure the node has a ShaderMaterial
    if (node.Material is not ShaderMaterial shaderMaterial)
    {
      GD.PrintErr($"Node {node.Name} does not have a ShaderMaterial!");
      return null;
    }

    // Create the Tween
    if (tween == null)
    {
      GD.PrintErr("Failed to create Tween.");
      return null;
    }

    // TweenMethod requires a callable, so we use a lambda function
    tween.TweenMethod(
      Callable.From<float>((value) => shaderMaterial.SetShaderParameter(shaderProperty, value)),
      valueStart,
      valueEnd,
      duration
    );

    return tween;
  }

  private static void SetShaderParameter(Node node, string shaderProperty, float value)
  {
    if (node is CanvasItem { Material: ShaderMaterial shaderMaterial } canvasItem)
    {
      shaderMaterial.SetShaderParameter(shaderProperty, value);
    }
  }
  
  public void SetLevelActive(Level level)
  {
    // Old level should be unloaded by now
    Main.Instance.UnloadingLevel = false;
    
    CurrentLevel = level;
    
    // Actually insert stuff to transfer into new level
    var newScene = GetTree().CurrentScene;
    Debug.Assert(newScene is Level);
    Debug.Assert(newScene == level);
    Debug.Assert(level.Spawn != null);

    if (_stuffToTransfer == null)
    {
      SpawnUser(level);
    }
    else
    {
      TransferUser(level);
    }
    CurrentLevel = level;
    level.Spawn.EnterRoom(level.PlayableCharacter);
    EmitSignal(SignalName.LevelReady);
  }
  
  private void SpawnUser(Level level)
  {
    // Spawn player
    var globalSpawnPosition = level.Spawn.MidPoint.GlobalPosition;
    var playerScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.Player];
    var player = playerScene.Instantiate<entities.player.PlayableCharacter>();
    player.GlobalPosition = globalSpawnPosition;
    level.AddChild(player);
    level.PlayableCharacter = player;
    player.Owner = level;

    // Spawn camera
    var playerCameraScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.PlayerCamera];
    var playerCamera = playerCameraScene.Instantiate<entities.player.PlayerCamera>();
    playerCamera.GlobalPosition = globalSpawnPosition;
    level.AddChild(playerCamera);
    level.PlayerCamera = playerCamera;
    playerCamera.Player = player;
    playerCamera.Owner = level;

    // Spawn def. staff
    var startingStaffScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.StartingStaff];
    var startingStaff = startingStaffScene.Instantiate<Staff>();
    startingStaff.GlobalPosition = globalSpawnPosition - new Vector2(64, 64);
    level.AddChild(startingStaff);
    startingStaff.Owner = level;
    
    level.Spawn.EnterRoom(player);
  }

  // For testing single rooms
  public static void SpawnUser(Room room)
  {
    // Spawn user in front of the first door we can find
    var doorToSpawnAt = room.TheoreticalDoorMarkers.First();
    var globalSpawnPosition = doorToSpawnAt.Value.GlobalPosition + doorToSpawnAt.Key.Opposite().ToVector() * 32;
    var playerScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.Player];
    var player = playerScene.Instantiate<entities.player.PlayableCharacter>();
    player.GlobalPosition = globalSpawnPosition;
    room.AddChild(player);
    player.Owner = room;

    // Spawn camera
    var playerCameraScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.PlayerCamera];
    var playerCamera = playerCameraScene.Instantiate<entities.player.PlayerCamera>();
    playerCamera.GlobalPosition = globalSpawnPosition;
    room.AddChild(playerCamera);
    playerCamera.Player = player;
    playerCamera.Owner = room;

    // Spawn def. staff
    var startingStaffScene = common.scenes.SceneLoader.Instance[common.scenes.SceneLoader.Scene.StartingStaff];
    var startingStaff = startingStaffScene.Instantiate<Staff>();
    startingStaff.GlobalPosition = globalSpawnPosition - new Vector2(64, 64);
    room.AddChild(startingStaff);
    startingStaff.Owner = room;

    // Manually set owner to player so we can start instantly
    startingStaff.PickupAreaOnBodyEntered(player);
    
    room.EnterRoom(player);
    playerCamera.UpdateRoom();
  }

  private void TransferUser(Level level)
  {
    if (_stuffToTransfer == null)
    {
      GD.PushError("Tried to transfer user to level, but no user to transfer!");
      return;
    }
    
    var stuff = (TransferableStuff)_stuffToTransfer;
    
    // Bring stuff into new level
    level.PlayableCharacter = stuff.Character;
    level.PlayableCharacter.GlobalPosition = level.Spawn.MidPoint.GlobalPosition;
    level.AddChild(level.PlayableCharacter);
    
    if (stuff.Staff != null)
    {
      level.ActiveStaff = stuff.Staff;
      level.ActiveStaff.GlobalPosition = level.PlayableCharacter.GlobalPosition - new Vector2(64, 64);
      level.AddChild(level.ActiveStaff);
      level.ActiveStaff.ClearOwner();
    }
    
    level.PlayerCamera = stuff.Camera;
    level.PlayerCamera.GlobalPosition = level.PlayableCharacter.GlobalPosition;
    level.AddChild(level.PlayerCamera);
    
    level.Spawn.EnterRoom(level.PlayableCharacter);
  }
  
  public LevelManager()
  {
    _instance = this;
  }
  
  public static LevelManager Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static LevelManager _instance;
  private static readonly object Padlock = new object();
}
