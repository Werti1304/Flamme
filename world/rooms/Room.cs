using Flamme.common.enums;
using Flamme.testing;
using Godot;
using System.Collections.Generic;
using Flamme.common.constant;
using Flamme.entities.env.Loot;

namespace Flamme.world.rooms;

[Tool]
public partial class Room : Area2D
{
  // Room size, must match tilemap, when in doubt press Generate Template
  [Export] public RoomSize Size;
  // Room type, affects generation
  [Export] public RoomType Type;
  // How likely this specific room is generated in comparison to others
  // To make a room super rare for example, make it 10
  // 0.999 is the workaround, cuz 1 will give you no slider at all in the editor :/
  [Export(PropertyHint.Range, "1,100,0.999")] public int RoomGenerationTickets = 100;
  [Export] public TileSet TileSet;
  
  [ExportSubgroup("Exits")]
  // All positions where the room may be entered/exited. Important for generation
  [Export] public RoomExit AllowedExits;

  public RoomExit ActualExits;
  
  private readonly List<Node2D> _lootList = [];

  private bool _cleared;
  public bool WasVisited;
  
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
  [Export] public TileMapLayer TileMap;
  [Export] public CollisionShape2D CollisionShape;

  [Signal] public delegate void PlayerEnteredEventHandler(PlayableCharacter playableCharacter);
  [Signal] public delegate void PlayerExitedEventHandler(PlayableCharacter playableCharacter);

  private List<Enemy> _enemies = [];
  // TODO List of entities, chests, etc.
  private PlayableCharacter _playableCharacter;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    BodyEntered += OnBodyEntered;
    BodyExited += OnBodyExited;

    CollisionMask = 0b1111;
    CollisionLayer = 0b1111;

    // Just to see stuff thats at 0
    TileMap.ZIndex = -1;

    foreach (var childNode in GetChildren())
    {
      if (childNode is Chest chest)
      {
        chest.GenerateLoot();
      }
    }
  }

  private Vector2 GetMidPoint()
  {
    return new Vector2(RoomSizeDict[Size].X / 2.0f, RoomSizeDict[Size].Y / 2.0f);
  }

  public Vector2 GetGlobalMidPoint()
  {
    return GlobalPosition + GetMidPoint() * 32.0f;
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
    LootGenerator.SpawnLootAt(_lootList, GetGlobalMidPoint());
    // foreach (var loot in _lootList)
    // {
    //   LevelManager.Instance.CurrentLevel.LootParent.AddChild(loot);
    //   // TODO 3 Calculate where to spawn loot
    //   loot.GlobalPosition = ;
    //   GD.Print($"Spawning loot: {loot.Name} at {loot.GlobalPosition} in room {Name} that has position {GlobalPosition}.");
    //   GD.Print($"Current player position: {LevelManager.Instance.CurrentLevel.PlayableCharacter.GlobalPosition}.");
    //   loot.SetProcessMode(ProcessModeEnum.Inherit);
    //   loot.SetVisible(true);
    // }
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
      case PlayableCharacter playableCharacter:
        WasVisited = true;
        SetCurrentRoom(playableCharacter);
        if (_enemies.Count > 0)
        {
          LockRoom(playableCharacter);
        }
        else
        {
          SetRoomCleared(false);
        }
        break;
      case Enemy e:
        // GD.Print($"Enemy found in room {Name}");
        // Only works if player goes into a room, not if it spawns there
        // -> No enemies in spawn room
        _enemies.Add(e);
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
      case PlayableCharacter:
        GD.Print($"Player exited room {Name}");
        SetRoomPassive();
        _playableCharacter = null;
        LevelManager.Instance.ExitedRoom(this);
        break;
      case Enemy e:
        _enemies.Remove(e);

        if (_enemies.Count == 0)
        {
          SetRoomCleared(true);
        }
        break;
    }
  }

  private void LockRoom(PlayableCharacter playableCharacter)
  {
    GD.Print($"Room {Name} Locked!");
      
    // Could replace with signals but idk
    foreach (var enemy in _enemies)
    {
      enemy.SetActive(playableCharacter);
    }
  }

  private void SetCurrentRoom(PlayableCharacter playableCharacter)
  {
    GD.Print($"Player entered Room {Name} with {_enemies.Count} enemies!");
    LevelManager.Instance.EnteredRoom(this);
    
    _playableCharacter = playableCharacter;
    
    if (_enemies.Count == 0)
    {
      foreach (var body in GetOverlappingBodies())
      {
        if (body is Enemy e)
        {
          _enemies.Add(e);
          GD.Print($"Thought room {Name} is empty, but had enemy {e.Name} inside!");
        }
      }

      if (_enemies.Count != 0)
      {
        LockRoom(playableCharacter);
      }
    }
  }

  private void SetRoomCleared(bool enemiesDefeated)
  {
    if (_cleared)
      return;
    _cleared = true;
    GD.Print($"Room {Name} Cleared!");

    if (Type == RoomType.Boss)
    {
      // TODO Preload all scenes?
      // Spawn warper to next level after clearing boss room
      var warperScene = GD.Load<PackedScene>(PathConstants.WarperScenePath);
      var warperNode = warperScene.Instantiate<entities.env.Warper>();
      warperNode.NewLevel = LevelManager.Instance.GetNextLevel();
      CallDeferred(Node.MethodName.AddChild, warperNode);
      warperNode.Position = GetMidPoint() * 32.0f;
    }
    else if (Type == RoomType.Pathway && enemiesDefeated)
    {
      SpawnLoot();
    }
  }

  private void SetRoomPassive()
  {
    GD.Print($"Door opened around Room {Name}");
    
    _playableCharacter = null;
    
    foreach (var enemy in _enemies)
    {
      enemy.SetPassive();
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

  public void CloseNotConnectedSides()
  {
    for (var x = 0; x < RoomSizeDict[Size].X; x++)
    {
      for (var y = 0; y < RoomSizeDict[Size].Y; y++)
      {
        // --- Check if we are at one of the exits ---
        if (x == 0)
        {
          switch (y)
          {
            case 5 when ActualExits.HasFlag(RoomExit.West):
            case 14 when ActualExits.HasFlag(RoomExit.West2):
            case 23 when ActualExits.HasFlag(RoomExit.West3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (x == RoomSizeDict[Size].X - 1)
        {
          switch (y)
          {
            case 5 when ActualExits.HasFlag(RoomExit.East):
            case 14 when ActualExits.HasFlag(RoomExit.East2):
            case 23 when ActualExits.HasFlag(RoomExit.East3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (y == 0)
        {
          switch (x)
          {
            case 8 when ActualExits.HasFlag(RoomExit.North):
            case 23 when ActualExits.HasFlag(RoomExit.North2):
            case 38 when ActualExits.HasFlag(RoomExit.North3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (y == RoomSizeDict[Size].Y - 1)
        {
          switch (x)
          {
            case 8 when ActualExits.HasFlag(RoomExit.South):
            case 23 when ActualExits.HasFlag(RoomExit.South2):
            case 38 when ActualExits.HasFlag(RoomExit.South3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }
      }
    }
  }
  
  // Defines how big the different room sizes are in pixels
  // This can't change anyways without changing every room, so no config neccessary
  private static readonly Godot.Collections.Dictionary<RoomSize, Vector2I> RoomSizeDict = new Godot.Collections.Dictionary<RoomSize, Vector2I>( ){
    { RoomSize.S1X1, new Vector2I(17, 11)},
    { RoomSize.S1X2, new Vector2I(17, 20)},
    { RoomSize.S2X1, new Vector2I(32, 11)},
    { RoomSize.S2X2, new Vector2I(32, 20)},
    { RoomSize.S3X1, new Vector2I(47, 20)},
    { RoomSize.S1X3, new Vector2I(17, 29)},
    { RoomSize.S3X3, new Vector2I(47, 29)}
  };

  private const int TemplateTileSourceId = 0;
  private static readonly Vector2I TemplateTileAtlasCoords = new Vector2I(1, 0);
  private static readonly Vector2I TemplateWallAtlasCoords = new Vector2I(0, 0);
  
  private void GenerateTemplate()
  {
    if (TileSet == null)
    {
      GD.PushError("TileSet not set!");
      return;
    }
    
    if (TileMap == null)
    {
      TileMap = new TileMapLayer();
      AddChild(TileMap);
      TileMap.Name = "TileMap";
      TileMap.TileSet = TileSet;
      TileMap.Owner = this;
    }

    if (CollisionShape == null)
    {
      CollisionShape = new CollisionShape2D();
      AddChild(CollisionShape);
      CollisionShape.Name = "CollisionShape";
      CollisionShape.Owner = this;
    }

    var errorAtlasCoords = new Vector2I(-1, -1);

    // Check if map is empty or only filled with template stuff
    for (var x = 0; x < RoomSizeDict[RoomSize.S3X3].X; x++)
    {
      for (var y = 0; y < RoomSizeDict[RoomSize.S3X3].Y; y++)
      {
        var cellCoords = new Vector2I(x, y);
        var cellSourceId = TileMap.GetCellSourceId(cellCoords);

        if (cellSourceId != -1 && cellSourceId != TemplateTileSourceId)
        {
          GD.Print(cellSourceId);
          return;
        }

        var atlasCoords = TileMap.GetCellAtlasCoords(cellCoords);

        if (atlasCoords != errorAtlasCoords
            && atlasCoords != TemplateTileAtlasCoords
            && atlasCoords != TemplateWallAtlasCoords)
        {
          GD.Print(atlasCoords);
          return;
        }
      }
    }

    TileMap.Clear();
    
    var shape = new RectangleShape2D();
    shape.Size = new Vector2(RoomSizeDict[Size].X * 32, RoomSizeDict[Size].Y * 32);
    CollisionShape.Shape = shape;
    CollisionShape.SetPosition(GetMidPoint() * 32);

    for (var x = 0; x < RoomSizeDict[Size].X; x++)
    {
      for (var y = 0; y < RoomSizeDict[Size].Y; y++)
      {
        // --- Check if we are at one of the exits ---
        if (x == 0)
        {
          switch (y)
          {
            case 5 when AllowedExits.HasFlag(RoomExit.West):
            case 14 when AllowedExits.HasFlag(RoomExit.West2):
            case 23 when AllowedExits.HasFlag(RoomExit.West3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (x == RoomSizeDict[Size].X - 1)
        {
          switch (y)
          {
            case 5 when AllowedExits.HasFlag(RoomExit.East):
            case 14 when AllowedExits.HasFlag(RoomExit.East2):
            case 23 when AllowedExits.HasFlag(RoomExit.East3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (y == 0)
        {
          switch (x)
          {
            case 8 when AllowedExits.HasFlag(RoomExit.North):
            case 23 when AllowedExits.HasFlag(RoomExit.North2):
            case 38 when AllowedExits.HasFlag(RoomExit.North3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (y == RoomSizeDict[Size].Y - 1)
        {
          switch (x)
          {
            case 8 when AllowedExits.HasFlag(RoomExit.South):
            case 23 when AllowedExits.HasFlag(RoomExit.South2):
            case 38 when AllowedExits.HasFlag(RoomExit.South3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }
        // --- Check Over ---

        // Replace with template
        TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateTileAtlasCoords);
      }
    }
  }
}