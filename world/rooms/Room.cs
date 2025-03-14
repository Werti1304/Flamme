using Flamme.common.enums;
using Flamme.testing;
using Godot;
using Flamme.common.constant;
using Flamme.entities.env.Loot;
using Flamme.ui;
using Flamme.world.doors;
using Flamme.world.generation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vector2I = Godot.Vector2I;

namespace Flamme.world.rooms;

[Tool]
public partial class Room : Area2D
{
  public static Room Current { get; private set; } = null;
  
  [Export] private bool SanityCheckTool // Button to create current RoomSize/Exits configuration on tilemap
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set
    {
      if(value)
        SanityCheck();
    }
  }
  
  [Export] public LevelType LevelType;
  // Room type, affects generation
  [Export] public RoomType Type;
  [Export] public bool CameraFixed = false;
  
  // How likely this specific room is generated in comparison to others
  // To make a room super rare for example, make it 10
  // 0.999 is the workaround, cuz 1 will give you no slider at all in the editor :/
  [Export(PropertyHint.Range, "1,100,0.999")] public int RoomGenerationTickets = 100;
  
  [Export] private bool UpdateDoorMarkersTool // Button to create current RoomSize/Exits configuration on tilemap
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set
    {
      if(value)
        UpdateDoorMarkers();
    }
  }
  // Gets automatically filled by pressing button, has to be done beforehand
  // Should NOT BE USED after world generation
  // All used ones get removed from here and added to Doors dictionary
  [Export] public Godot.Collections.Dictionary<Cardinal, DoorMarker> TheoreticalDoorMarkers = [];
  
  // Gets filled during world generation
  public readonly Dictionary<Cardinal, Door> Doors = [];
  
  [ExportGroup("Generation")]
  [Export]
  public bool GenerateTemplateTool // Button to create current RoomSize/Exits configuration on tilemap
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set
    {
      if(value)
        GenerateTemplate();
    }
  }

  [ExportGroup("Meta")] 
  [Export] public TileMapLayer FloorTileMap;
  [Export] public TileMapLayer PropsTileMap;
  [Export] public TileMapLayer OuterWallTileMap;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Node2D DoorMarkerParent;
  [Export] public Node2D MidPoint;

  [Signal] public delegate void PlayerEnteredEventHandler(PlayableCharacter playableCharacter);
  [Signal] public delegate void PlayerExitedEventHandler(PlayableCharacter playableCharacter);

  public bool WasVisited;
  public readonly List<Enemy> Enemies = [];

  private PlayableCharacter _playableCharacter;
  private List<Node2D> _lootList = [];
  private bool _cleared;
  
  public override void _Ready()
  {
    // TODO Uncomment once tilemaps fixed
    // ExportMetaNonNull.Check(this);
    
    BodyEntered += OnBodyEntered;
    BodyExited += OnBodyExited;

    CollisionMask = 0b1111;
    CollisionLayer = 0b1111;

    foreach (var childNode in GetChildren())
    {
      if (childNode is Chest chest)
      {
        chest.GenerateLoot();
      }
    }
  }

  public void AddLoot(Node2D loot)
  {
    if (loot == null)
    {
      GD.PushError($"Tried to add null loot to room {Name}!");
      return;
    }
    loot.SetProcessMode(ProcessModeEnum.Disabled);
    loot.SetVisible(false);
    _lootList.Add(loot);
    // GD.Print($"Generated (not spawned) loot: {loot.Name} at {loot.GlobalPosition} in room {Name} that has position {GlobalPosition}.");
  }

  private void SpawnLoot()
  {
    LootGenerator.SpawnLootAt(_lootList, MidPoint.GlobalPosition);
    _lootList.Clear();
  }
  
  private void OnBodyEntered(Node2D body)
  {
    if (Main.Instance.ShuttingDown)
    {
      return;
    }
    
    switch (body)
    {
      case PlayableCharacter p:
        if (Current == null)
        {
          // Only a fallback for dev worlds and spawns
          GD.Print($"Player entered room {Name} for on failsafe! (This should only happen on non-generated worlds)");
          EnterRoom(p);
        }
        break;
      case Enemy e:
        // GD.Print($"Enemy found in room {Name}");
        // Only works if player goes into a room, not if it spawns there
        // -> No enemies in spawn room
        Enemies.Add(e);
        break;
    }
  }
  
  private void OnBodyExited(Node2D body)
  {
    if (Main.Instance.ShuttingDown)
    {
      return;
    }
    
    switch (body)
    {
      case Enemy e:
        Enemies.Remove(e);

        if (Enemies.Count == 0)
        {
          SetRoomCleared(true);
        }
        break;
    }
  }

  private void LockRoom(PlayableCharacter playableCharacter)
  {
    GD.Print($"Room {Name} Locked!");
    
    // playableCharacter.GlobalPosition += 
    //   playableCharacter.GlobalPosition.DirectionTo(GetGlobalMidPoint()) * 32.0f;

    foreach (var door in Doors.Values)
    {
      door.Lock();
    }
      
    // Could replace with signals but idk
    foreach (var enemy in Enemies)
    {
      enemy.SetActive(playableCharacter);
    }
  }

  public void EnterRoom(PlayableCharacter playableCharacter)
  {
    GD.Print($"Player entered Room {Name} with {Enemies.Count} enemies!");
    Current = this;
    
    Hud.Instance.Minimap.UpdateCurrentRoom();
    Level.Current.PlayerCamera.UpdateRoom();
    _playableCharacter = playableCharacter;
    
    if (Enemies.Count == 0)
    {
      foreach (var body in GetOverlappingBodies())
      {
        if (body is Enemy e)
        {
          Enemies.Add(e);
          GD.Print($"Thought room {Name} is empty, but had enemy {e.Name} inside!");
        }
      }
    }
    
    WasVisited = true;
    if (Enemies.Count > 0)
    {
      LockRoom(playableCharacter);
    }
    else
    {
      SetRoomCleared(false);
    }
  }
  
  public void LeaveRoom()
  {
    _playableCharacter = null;
    
    foreach (var enemy in Enemies)
    {
      enemy.SetPassive();
    }
  }

  private void SetRoomCleared(bool enemiesDefeated)
  {
    if (_cleared)
      return;
    _cleared = true;

    foreach (var door in Doors.Values)
    {
      door.OpenByClearingRoom();
    }
    
    GD.Print($"Room {Name} Cleared!");

    if (Type == RoomType.Boss)
    {
      // TODO Preload all scenes?
      // Spawn warper to next level after clearing boss room
      var warperScene = GD.Load<PackedScene>(PathConstants.WarperScenePath);
      var warperNode = warperScene.Instantiate<entities.env.Warper>();
      CallDeferred(Node.MethodName.AddChild, warperNode);
      warperNode.Position = MidPoint.Position * 32.0f;
    }
    else if (Type == RoomType.Pathway && enemiesDefeated)
    {
      SpawnLoot();
    }
  }
  
  public void GenerateLoot()
  {
    if (Type == RoomType.Pathway)
    {
      _lootList = LootGenerator.Instance.GeneratePathwayLoot();
    }
  }

  public override void _Notification(int what)
  {
    // NOTIFICATION_PREDELETE, Destructor-Equivalent for Nodes
    // https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-constant-notification-predelete
    if (what != NotificationPredelete)
    {
      return;
    }

    foreach (var node in _lootList)
    {
      node.QueueFree();
    }
  }
  
  // Context: Editor Tool
  private void UpdateDoorMarkers()
  {
    TheoreticalDoorMarkers.Clear();
    foreach (var doorMarker in DoorMarkerParent.GetChildren())
    {
      if (doorMarker is DoorMarker marker)
      {
        if (TheoreticalDoorMarkers.TryAdd(marker.FacingDirection, marker))
        {
          continue;
        }
        GD.PushWarning($"Found duplicate DoorMarker {doorMarker.Name} in DoorMarkerParent of room {Name}!");
      }
      else
      {
        GD.PushWarning($"Found non-DoorMarker child {doorMarker.Name} in DoorMarkerParent of room {Name}!");
      }
    }
  }
  
  // Context: Editor Tool
  private void GenerateTemplate()
  {
    var tileset = LevelType switch
    {
      LevelType.Prison => GD.Load<TileSet>(PathConstants.PrisonTileSetPath),
      _ => throw new ArgumentOutOfRangeException()
    };

    if (FloorTileMap == null || !IsInstanceValid(FloorTileMap))
    {
      FloorTileMap = new TileMapLayer();
      AddChild(FloorTileMap);
      FloorTileMap.TileSet = tileset;
      FloorTileMap.Name = "Floor";
      FloorTileMap.Owner = this;
      FloorTileMap.ZIndex = -1;
    }
    
    if (PropsTileMap == null)
    {
      PropsTileMap = new TileMapLayer();
      AddChild(PropsTileMap);
      PropsTileMap.TileSet = tileset;
      PropsTileMap.Name = "Props";
      PropsTileMap.Owner = this;
    }

    if (OuterWallTileMap == null)
    {
      OuterWallTileMap = new TileMapLayer();
      AddChild(OuterWallTileMap);
      OuterWallTileMap.TileSet = tileset;
      OuterWallTileMap.Name = "Outer Wall";
      OuterWallTileMap.Owner = this;
    }
    
    if (MidPoint == null)
    {
      MidPoint = new Node2D();
      AddChild(MidPoint);
      MidPoint.Name = "MidPoint";
      MidPoint.Owner = this;
    }

    if (DoorMarkerParent == null)
    {
      DoorMarkerParent = new Node2D();
      AddChild(DoorMarkerParent);
      DoorMarkerParent.Name = "DoorMarkerParent";
      DoorMarkerParent.Owner = this;
    }
    
    if (CollisionShape == null)
    {
      CollisionShape = new CollisionShape2D();
      AddChild(CollisionShape);
      CollisionShape.Name = "CollisionShape";
      CollisionShape.Owner = this;
      
      var shape = new RectangleShape2D();
      CollisionShape.Shape = shape;
      CollisionShape.Hide();
      
      // Just for simpler room building, unused in code
      var node = new Node2D();
      AddChild(node);
      node.Name = "Loot";
      node.Owner = this;
      
      // just for simpler room building, unused in code
      node = new Node2D();
      AddChild(node);
      node.Name = "Enemies";
      node.Owner = this;
    }
  }
  
  private static readonly Vector2I MaxRoomSize = new Vector2I(64 * 32, 32 * 32);
  private static readonly Vector2I MinRoomSize = new Vector2I(17 * 32, 10 * 32);
  
  private void SanityCheck()
  {
    ExportMetaNonNull.Check(this);
    
    Debug.Assert(TheoreticalDoorMarkers.Count > 0, "No door markers in room!");
    UpdateDoorMarkers();
    
    var shape = CollisionShape.Shape as RectangleShape2D;
    Debug.Assert(shape != null, "CollisionShape is not a rectangle!");
    Debug.Assert(shape.Size.X >= MinRoomSize.X && shape.Size.Y >= MinRoomSize.Y, 
      $"Room size is of {shape.Size} too small, must at least be {MinRoomSize} tiles!");
    Debug.Assert(shape.Size.X <= MaxRoomSize.X && shape.Size.Y <= MaxRoomSize.Y,
      $"Room size of {shape.Size} is too big, must at most be {MaxRoomSize} tiles!");
    
    Debug.Assert(!FloorTileMap.TileMapData.IsEmpty(), "FloorTileMap is empty!");
    Debug.Assert(!OuterWallTileMap.TileMapData.IsEmpty(), "OuterWallTileMap is empty!");
  }
}