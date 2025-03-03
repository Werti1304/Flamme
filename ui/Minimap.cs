using Flamme.common.enums;
using Flamme.testing;
using Flamme.world.generation;
using Godot;

namespace Flamme.ui;

public partial class Minimap : GridContainer
{
  [ExportGroup("Meta")] [Export] public Texture2D PathwayTexture;
  [Export] public Texture2D SpawnTexture;
  [Export] public Texture2D TreasureTexture;
  [Export] public Texture2D BossTexture;
  [Export] public Texture2D ShopTexture;

  private readonly TextureRect[,] _grid = new TextureRect[16, 16];

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    // Needed cuz of grid container shenanigans
    for (var y = 0; y < Columns; y++)
    {
      for (var x = 0; x < Columns; x++)
      {
        var textureRect = new TextureRect();
        AddChild(textureRect);
        textureRect.Owner = this;
        _grid[x, y] = textureRect;
      }
    }
  }

  public void Update(Level level)
  {
    var lowestX = int.MaxValue;
    var lowestY = int.MaxValue;

    for (var x = 0; x < level.Grid.GetLength(0); x++)
    {
      for (var y = 0; y < level.Grid.GetLength(0); y++)
      {
        var room = level.Grid[x, y];
        if (room == null)
          continue;

        if (lowestX > x)
        {
          lowestX = x;
        }

        if (lowestY > y)
        {
          lowestY = y;
        }
      }
    }

    for (var x = 0; x < level.Grid.GetLength(0); x++)
    {
      for (var y = 0; y < level.Grid.GetLength(0); y++)
      {
        var room = level.Grid[x, y];
        if (room == null)
          continue;

        var gridX = x - lowestX;
        var gridY = y - lowestY;
        GD.Print($"Set grid at {gridX}, {gridY}");

        switch (room.Type)
        {
          case RoomType.Pathway:
          default:
            _grid[gridX, gridY].Texture = PathwayTexture;
            break;
          case RoomType.Spawn:
            _grid[gridX, gridY].Texture = SpawnTexture;
            break;
          case RoomType.Treasure:
            _grid[gridX, gridY].Texture = TreasureTexture;
            break;
          case RoomType.Shop:
            _grid[gridX, gridY].Texture = ShopTexture;
            break;
          case RoomType.Boss:
            _grid[gridX, gridY].Texture = BossTexture;
            break;
        }
      }
    }
  }
}