using Flamme.common.enums;
using Flamme.testing;
using Flamme.world.generation;
using Godot;
using Room = Flamme.world.rooms.Room;

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

  private int _lowestX = int.MaxValue;
  private int _lowestY = int.MaxValue;

  public void SetCurrentRoom(Level level, Room playerRoom)
  {
    for (var x = 0; x < level.Grid.GetLength(0); x++)
    {
      for (var y = 0; y < level.Grid.GetLength(0); y++)
      {
        var room = level.Grid[x, y];
        if (room == null)
          continue;

        var gridX = x - _lowestX;
        var gridY = y - _lowestY;
        
        _grid[gridX, gridY].Modulate = room == playerRoom ? Colors.White : Colors.LightGray;
      }
    }
  }

  // TODO 1 Performance
  // Can be made more efficient with less updates to the whole thing
  // but for now its ok
  public void Update(Level level)
  {
    _lowestX = int.MaxValue;
    _lowestY = int.MaxValue;

    for (var x = 0; x < level.Grid.GetLength(0); x++)
    {
      for (var y = 0; y < level.Grid.GetLength(0); y++)
      {
        var room = level.Grid[x, y];
        if (room == null)
          continue;

        if (_lowestX > x)
        {
          _lowestX = x;
        }

        if (_lowestY > y)
        {
          _lowestY = y;
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

        var gridX = x - _lowestX;
        var gridY = y - _lowestY;
        //GD.Print($"Set grid at {gridX}, {gridY}");

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