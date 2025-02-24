using Flamme.testing;
using Godot;
using Godot.Collections;
using System;

namespace Flamme.world.rooms;

[Tool]
public partial class Room : Area2D
{
  [Export]
  public bool GenerateTemplateTool // Button to create current RoomSize/Exits configuration on tilemap
  {
    get => false;
    // ReSharper disable once ValueParameterNotUsed
    set => GenerateTemplate();
  }

  // Room size, must match tilemap, when in doubt press Generate Template
  [Export] public Size RoomSize;
  // Room type, affects generation
  [Export] public Type RoomType;
  // How likely this specific room is generated in comparison to others
  // To make a room super rare for example, make it 10
  [Export] public int RoomGenerationTickets = 100; 
  
  [ExportSubgroup("Exits")]
  // All positions where the room may be entered/exited. Important for generation
  [Export] public Exit AllowedExits;
  
  [ExportGroup("Meta")] 
  [Export] public TileMapLayer TileMap;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Node2D DoorsParent;

  public enum Size
  {
    S1X1,
    S1X2,
    S2X1,
    S2X2,
    S3X1,
    S1X3,
    S3X3
  }
  
  [Flags]
  public enum Exit
  {
    North  = 0x1,   // 1
    South  = 0x2,   // 2
    West   = 0x4,   // 4
    East   = 0x8,   // 8
    North2 = 0x10, // 16
    South2 = 0x20, // 32
    West2  = 0x40, // 64
    East2  = 0x80, // 128
    North3 = 0x100, // 256
    South3 = 0x200, // 512
    West3  = 0x400, // 1024
    East3  = 0x800,  // 2048
    Size   = 0x801
  }

  public enum Type
  {
    Pathway,
    Treasure,
    Shop,
    Smithy,
    Boss,
  }
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public Vector2 GetMidPoint()
  {
    return new Vector2(RoomSizeDict[RoomSize].X / 2.0f, RoomSizeDict[RoomSize].Y / 2.0f);
  }
  
  // Defines how big the different room sizes are in pixels
  // This can't change anyways without changing every room, so no config neccessary
  public static readonly Dictionary<Size, Vector2I> RoomSizeDict = new Dictionary<Size, Vector2I>( ){
    { Size.S1X1, new Vector2I(17, 11)},
    { Size.S1X2, new Vector2I(17, 20)},
    { Size.S2X1, new Vector2I(32, 11)},
    { Size.S2X2, new Vector2I(32, 20)},
    { Size.S3X1, new Vector2I(47, 20)},
    { Size.S1X3, new Vector2I(17, 29)},
    { Size.S3X3, new Vector2I(47, 29)}
  };

  private const int TemplateTileSourceId = 0;
  private static readonly Vector2I TemplateTileAtlasCoords = new Vector2I(1, 0);
  private static readonly Vector2I TemplateWallAtlasCoords = new Vector2I(0, 0);
  
  private void GenerateTemplate()
  {
    if (TileMap == null)
      return;

    var errorAtlasCoords = new Vector2I(-1, -1);

    // Check if map is empty or only filled with template stuff
    for (var x = 0; x < RoomSizeDict[Size.S3X3].X; x++)
    {
      for (var y = 0; y < RoomSizeDict[Size.S3X3].Y; y++)
      {
        var cellCoords = new Vector2I(x, y);
        var cellSourceID = TileMap.GetCellSourceId(cellCoords);

        if (cellSourceID != -1 && cellSourceID != TemplateTileSourceId)
        {
          GD.Print(cellSourceID);
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
    shape.Size = new Vector2(RoomSizeDict[RoomSize].X * 32, RoomSizeDict[RoomSize].Y * 32);
    CollisionShape.Shape = shape;
    CollisionShape.SetPosition(GetMidPoint() * 32);

    for (var x = 0; x < RoomSizeDict[RoomSize].X; x++)
    {
      for (var y = 0; y < RoomSizeDict[RoomSize].Y; y++)
      {
        // --- Check if we are at one of the exits ---
        if (x == 0)
        {
          switch (y)
          {
            case 5 when AllowedExits.HasFlag(Exit.West):
            case 14 when AllowedExits.HasFlag(Exit.West2):
            case 23 when AllowedExits.HasFlag(Exit.West3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (x == RoomSizeDict[RoomSize].X - 1)
        {
          switch (y)
          {
            case 5 when AllowedExits.HasFlag(Exit.East):
            case 14 when AllowedExits.HasFlag(Exit.East2):
            case 23 when AllowedExits.HasFlag(Exit.East3):
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
            case 8 when AllowedExits.HasFlag(Exit.North):
            case 23 when AllowedExits.HasFlag(Exit.North2):
            case 38 when AllowedExits.HasFlag(Exit.North3):
              break;
            default:
              TileMap.SetCell(new Vector2I(x, y), TemplateTileSourceId, TemplateWallAtlasCoords);
              continue;
          }
        }

        if (y == RoomSizeDict[RoomSize].Y - 1)
        {
          switch (x)
          {
            case 8 when AllowedExits.HasFlag(Exit.South):
            case 23 when AllowedExits.HasFlag(Exit.South2):
            case 38 when AllowedExits.HasFlag(Exit.South3):
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