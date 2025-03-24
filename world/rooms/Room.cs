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
using System.Globalization;
using System.Linq;
using Vector2I = Godot.Vector2I;

namespace Flamme.world.rooms;

[Tool]
public partial class Room : Area2D
{
  public static Room Current { get; private set; } = null;
  
  [Export] private bool CommitChangesTool // Button to create current RoomSize/Exits configuration on tilemap
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
  [Export] public bool CameraFixedX = false;
  [Export] public bool CameraFixedY = false;
  
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
  public bool FillRoofTool // Button to fill in roof based on other tilemaps and collision shape
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set
    {
      if(value)
        FillRoof();
    }
  }

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
  [Export] public TileMapLayer RoofTileMap;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Node2D DoorMarkerParent;
  [Export] public Node2D MidPoint;

  [Signal] public delegate void PlayerEnteredEventHandler(PlayableCharacter playableCharacter);
  [Signal] public delegate void PlayerExitedEventHandler(PlayableCharacter playableCharacter);

  public bool WasVisited = false;
  public readonly List<Enemy> Enemies = [];

  private PlayableCharacter _playableCharacter;
  private List<Node2D> _lootList = [];
  private bool _cleared;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
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

    if (GetTree().CurrentScene == this)
    {
      // We are just testing this room, spawn in player, etc.
      LevelManager.SpawnUser(this);
      Hud.Instance.Show();
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

        if (_playableCharacter != null)
        {
          e.SetActive(_playableCharacter);
        }
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
    WasVisited = true;
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
    
    if (Enemies.Count > 0)
    {
      LockRoom(playableCharacter);
    }
    else
    {
      SetRoomCleared(false);
    }
    
    if (!IsInstanceValid(Level.Current))
    {
      GD.PushWarning("Level is not valid, cannot update camera and minimap!");
      return;
    }
    Hud.Instance.Minimap.UpdateCurrentRoom();
    Level.Current.PlayerCamera.UpdateRoom();
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
    TileSet FloorTileSet = null;
    TileSet PropsTileSet = null;
    TileSet OuterWallTileSet = null;

    switch (LevelType)
    {
      case LevelType.Prison:
      {
        FloorTileSet = GD.Load<TileSet>(PathConstants.PrisonFloorTileSetPath);
        PropsTileSet = GD.Load<TileSet>(PathConstants.PrisonPropsTileSetPath);
        OuterWallTileSet = GD.Load<TileSet>(PathConstants.PrisonWallTileSetPath);
      }
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    if (FloorTileMap == null || !IsInstanceValid(FloorTileMap))
    {
      FloorTileMap = new TileMapLayer();
      AddChild(FloorTileMap);
      FloorTileMap.TileSet = FloorTileSet;
      FloorTileMap.Name = "Floor";
      FloorTileMap.Owner = this;
      FloorTileMap.ZIndex = -1;
    }
    
    if (PropsTileMap == null)
    {
      PropsTileMap = new TileMapLayer();
      AddChild(PropsTileMap);
      PropsTileMap.TileSet = PropsTileSet;
      PropsTileMap.Name = "Props";
      PropsTileMap.Owner = this;
    }

    if (OuterWallTileMap == null)
    {
      OuterWallTileMap = new TileMapLayer();
      AddChild(OuterWallTileMap);
      OuterWallTileMap.TileSet = OuterWallTileSet;
      OuterWallTileMap.Name = "Wall";
      OuterWallTileMap.Owner = this;
    }
    
    if (RoofTileMap == null)
    {
      RoofTileMap = new TileMapLayer();
      AddChild(RoofTileMap);
      RoofTileMap.TileSet = OuterWallTileSet;
      RoofTileMap.Name = "Roof (Auto)";
      RoofTileMap.Owner = this;
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

    if (DoorMarkerParent.GetChildren().Count == 0)
    {
      // Add all 4 possible door marker. Can delete in scene if not needed
      var doorMarkerScene = GD.Load<PackedScene>(PathConstants.DoorMarkerScenePath);
      foreach (var cardinal in Enum.GetValues<Cardinal>())
      {
        var marker = doorMarkerScene.Instantiate<DoorMarker>();
        DoorMarkerParent.AddChild(marker);
        marker.Name = $"DoorMarker {cardinal}";
        marker.FacingDirection = cardinal;
        marker.Owner = this;
      }
    }
    
    if (CollisionShape == null)
    {
      CollisionShape = new CollisionShape2D();
      AddChild(CollisionShape);
      CollisionShape.Name = "CollisionShape";
      CollisionShape.Owner = this;
      
      var shape = new RectangleShape2D();
      shape.Size = MinRoomSize;
      CollisionShape.Shape = shape;
      CollisionShape.Position = new Vector2(MinRoomSize.X / 2.0f, MinRoomSize.Y / 2.0f);
      
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
      
      // just for simpler room building, unused in code
      node = new Node2D();
      AddChild(node);
      node.Name = "Tile Entities";
      node.Owner = this;
    }
  }
  
  private static readonly Vector2I MaxRoomSize = new Vector2I(64 * 32, 32 * 32);
  private static readonly Vector2I MinRoomSize = new Vector2I(17 * 32, 10 * 32);
  
  private void SanityCheck()
  {
    var clear = ExportMetaNonNull.Check(this);

    if (!clear)
    {
      GD.PushError("One or more required exports are missing, cannot continue!");
      return;
    }

    var shouldBeName = SceneFilePath.Split('/').Last().Replace(".tscn", "");
    shouldBeName = new CultureInfo("en").TextInfo.ToTitleCase(shouldBeName.ToLower().Replace("_", " ")).Replace(" ", "");
    if (shouldBeName != Name)
    {
      Name = shouldBeName;
      GD.Print($"Room name wrong, changed it from {shouldBeName} to {Name}!");
    }

    var shape = CollisionShape.Shape as RectangleShape2D;
    Debug.Assert(shape != null, "CollisionShape is not a rectangle!");
    var roomSize = (Vector2I)(shape.Size / 32);
    
    for (var x = 0; x < roomSize.X; x++)
    {
      for (var y = 0; y < roomSize.Y; y++)
      {
        var coords = new Vector2I(x, y);
        if (PropsTileMap.GetCellSourceId(coords) != -1)
        {
          FloorTileMap.SetCell(coords, RoomGenConstants.FloorTileSourceId, RoomGenConstants.UnderPropFloor);
        }
      }
    }
    
    Debug.Assert(MidPoint.Position == Vector2.Zero, "MidPoint wasn't moved yet!");
    
    UpdateDoorMarkers();
    Debug.Assert(TheoreticalDoorMarkers.Count > 0, "No door markers in room!");
    
    Debug.Assert(shape.Size.X >= MinRoomSize.X && shape.Size.Y >= MinRoomSize.Y, 
      $"Room size is of {shape.Size} too small, must at least be {MinRoomSize} tiles!");
    Debug.Assert(shape.Size.X <= MaxRoomSize.X && shape.Size.Y <= MaxRoomSize.Y,
      $"Room size of {shape.Size} is too big, must at most be {MaxRoomSize} tiles!");
    
    Debug.Assert(!FloorTileMap.TileMapData.IsEmpty(), "FloorTileMap is empty!");
    Debug.Assert(!OuterWallTileMap.TileMapData.IsEmpty(), "OuterWallTileMap is empty!");
    
    GD.Print("Sanity check done!");
  }

  private void FillRoof()
  {
    RoofTileMap.Clear();
    
    // Get Region that needs to be filled in

    var shape = CollisionShape.Shape as RectangleShape2D;
    Debug.Assert(shape != null, "CollisionShape is not a rectangle!");
    var roomSize = (Vector2I)(shape.Size / 32);

    var alreadyFilledRect = new RectangleShape2D();

    var toFill = new Godot.Collections.Array<Vector2I>();

    // TODO Fill list with to be terrained tilemap positions, do setcellterrainconnect at the bottom
    // try out how to get rid of pescy outer tile problems
    for (var x = -1; x <= roomSize.X; x++)
    {
      for (var y = -1; y <= roomSize.Y; y++)
      {
        var coords = new Vector2I(x, y);

        // If tile is neither empty nor a roof tile, break
        if (FloorTileMap.GetCellSourceId(coords) is not -1 
            || OuterWallTileMap.GetCellSourceId(coords) is not -1 and not RoomGenConstants.RoofTerrainSourceId)
        {
          continue;
        }
        toFill.Add(coords);
      }
    }

    RoofTileMap.SetCellsTerrainConnect(toFill, 0, 0, true);

    // Do the still water in minecraft equivalent with terrain tiles
    for (var x = -1; x <= roomSize.X; x++)
    {
      for (var y = -1; y <= roomSize.Y; y++)
      {
        if (x == -1 || x == roomSize.X || y == -1 || y == roomSize.Y)
        {
          var coords = new Vector2I(x, y);
          RoofTileMap.SetCell(coords, -1);
        }
      }
    }
  }
}