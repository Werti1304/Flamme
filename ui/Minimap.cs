using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.common.input;
using Flamme.testing;
using Flamme.world;
using Flamme.world.generation;
using Godot;
using System.Diagnostics;
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
    
    Modulate = Colors.Transparent;
  }

  private const ulong MaxTimeMsec = 200;
  private ulong _timeMsec = 0;
  public override void _Input(InputEvent @event)
  {
    if (@event.IsActionPressed(PlayerInputMap.MapKey))
    {
      if (Modulate != Colors.White)
      {
        // To let the map on screen when pressing Tab for < MaxTimeMsec
        // If pressed rapidly again, vanish it
        _timeMsec = Time.Singleton.GetTicksMsec();
      }
      Modulate = Colors.White;
    }
    else if (@event.IsActionReleased(PlayerInputMap.MapKey))
    {
      if (Time.Singleton.GetTicksMsec() - _timeMsec < MaxTimeMsec)
      {
        return;
      }
      Modulate = Colors.Transparent;
    }
  }

  private int _lowestX = int.MaxValue;
  private int _lowestY = int.MaxValue;

  public void UpdateCurrentRoom()
  {
    var level = LevelManager.Instance.CurrentLevel;
    var playerRoom = Room.Current;
    for (var x = 0; x < level.Grid.GetLength(0); x++)
    {
      for (var y = 0; y < level.Grid.GetLength(0); y++)
      {
        var room = level.Grid[x, y];
        if (room == null)
          continue;

        var gridX = x - _lowestX;
        var gridY = y - _lowestY;
        
        var currentRoomColor = new Color(1.0f, 1.0f, 1.0f, 0.7f);
        var visitedRoomColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        var unvisitedRoomColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        var hiddenRoomColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        if (room == playerRoom)
        {
          _grid[gridX, gridY].Modulate = currentRoomColor;
        }
        else if(room.WasVisited || DebugToggles.MinimapSeeAll)
        {
          _grid[gridX, gridY].Modulate = visitedRoomColor;
        }
        else
        {
          // Check 4-neighbourhood
          if (level.Grid[x + 1, y] != null && level.Grid[x + 1, y].WasVisited
              || level.Grid[x, y + 1] != null && level.Grid[x, y + 1].WasVisited
              || level.Grid[x - 1, y] != null && level.Grid[x - 1, y].WasVisited
              || level.Grid[x, y - 1] != null && level.Grid[x, y - 1].WasVisited)
          {
            _grid[gridX, gridY].Modulate = unvisitedRoomColor;
            continue;
          }
          _grid[gridX, gridY].Modulate = hiddenRoomColor;
        }
      }
    }
  }
  
  public void UpdateLevel()
  {
    foreach (var textureRect in _grid)
    {
      textureRect.Texture = null;
    }
    
    var level = LevelManager.Instance.CurrentLevel;
    
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