using Flamme.common.enums;
using Flamme.testing;
using Flamme.world.generation;
using Godot;
using System;

public partial class Minimap : GridContainer
{
  [ExportGroup("Meta")]
  [Export] public Texture2D PathwayTexture;
  [Export] public Texture2D SpawnTexture;
  [Export] public Texture2D TreasureTexture;
  [Export] public Texture2D BossTexture;
  [Export] public Texture2D ShopTexture;
  
  public TextureRect[,] Grid = new TextureRect[16, 16];

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    for (int y = 0; y < Columns; y++)
    {
      for (int x = 0; x < Columns; x++)
      {
        var textureRect = new TextureRect();
        AddChild(textureRect);
        textureRect.Owner = this;
        Grid[x, y] = textureRect;
      }
    }
  }

  public void Update(Level level)
  {
    var lowestX = int.MaxValue;
    var lowestY = int.MaxValue;
    
    for (var y = 0; y < level.Grid.GetLength(0); y++)
    {
      for (var x = 0; x < level.Grid.GetLength(0); x++)
      {
        var room = level.Grid[x, y];
        if(room == null)
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
    
    for (var y = 0; y < level.Grid.GetLength(0); y++)
    {
      for (var x = 0; x < level.Grid.GetLength(0); x++)
      {
        var room = level.Grid[x, y];
        if(room == null)
          continue;
        
        var gridX = x - lowestX;
        var gridY = y - lowestY;
        GD.Print($"Set grid at {gridX}, {gridY}");

        switch (room.Type)
        {
          case RoomType.Pathway:
          default:
            Grid[gridX, gridY].Texture = PathwayTexture;
            break;
          case RoomType.Spawn:
            Grid[gridX, gridY].Texture = SpawnTexture;
            break;
          case RoomType.Treasure:
            Grid[gridX, gridY].Texture = TreasureTexture;
            break;
          case RoomType.Shop:
            Grid[gridX, gridY].Texture = ShopTexture;
            break;
          case RoomType.Boss:
            Grid[gridX, gridY].Texture = BossTexture;
            break;
        }
      }
    }
  }
}
