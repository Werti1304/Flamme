using Flamme.common.enums;
using Flamme.testing;
using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Environment = Godot.Environment;

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
  [Export] public int RoomGenerationTickets = 100;
  [Export] public TileSet TileSet;
  
  [ExportSubgroup("Exits")]
  // All positions where the room may be entered/exited. Important for generation
  [Export] public RoomExit AllowedExits;
  
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

  private List<Enemy> _enemies = new List<Enemy>();
  // TODO List of entities, chests, etc.
  private PlayableCharacter _playableCharacter;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    BodyEntered += OnBodyEntered;
    BodyExited += OnBodyExited;

    CollisionMask = 0b1110;
    CollisionLayer = 0b1110;
  }

  private Vector2 GetMidPoint()
  {
    return new Vector2(RoomSizeDict[Size].X / 2.0f, RoomSizeDict[Size].Y / 2.0f);
  }

  public Vector2 GetGlobalMidPoint()
  {
    return GlobalPosition + GetMidPoint() * 32.0f;
  }
  
  private void OnBodyEntered(Node2D body)
  {
    switch (body)
    {
      case PlayableCharacter playableCharacter:
        SetRoomActive(playableCharacter);
        break;
      case Enemy e:
        GD.Print($"Enemy found in room {Name}");
        // Only works if player goes into a room, not if it spawns there
        // -> No enemies in spawn room
        _enemies.Add(e);
        break;
    }
  }
  
  private void OnBodyExited(Node2D body)
  {
    switch (body)
    {
      case PlayableCharacter playableCharacter:
        GD.Print($"Player exited room {Name}");
        SetRoomPassive();
        break;
      case Enemy e:
        _enemies.Remove(e);

        if (_enemies.Count == 0)
        {
          SetRoomCleared();
        }
        break;
    }
  }

  private void SetRoomActive(PlayableCharacter playableCharacter)
  {
    GD.Print($"Player entered Room {Name}");
    _playableCharacter = playableCharacter;
    if (GetViewport().GetCamera2D() is PlayerCamera camera)
    {
      camera.SetRoom(this);
    }

    if (_enemies.Count == 0)
    {
      SetRoomCleared();
      return;
    }
    
    GD.Print($"Room {Name} Locked!");
      
    // Could replace with signals but idk
    foreach (var enemy in _enemies)
    {
      enemy.SetActive(playableCharacter);
    }
  }

  private void SetRoomCleared()
  {
    GD.Print($"Room {Name} Cleared!");
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
  
  // Defines how big the different room sizes are in pixels
  // This can't change anyways without changing every room, so no config neccessary
  public static readonly Godot.Collections.Dictionary<RoomSize, Vector2I> RoomSizeDict = new Godot.Collections.Dictionary<RoomSize, Vector2I>( ){
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