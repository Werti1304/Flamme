using Godot;
using System;
using Flamme.common.enums;
using Flamme.entities.env;

public partial class Coin : Area2D, IPursePickup
{
  [Export] private int _count = 1;
  [ExportGroup("Textures")] [Export] private AtlasTexture _coin1;
  [ExportGroup("Textures")] [Export] private AtlasTexture _coin20;
  [ExportGroup("Textures")] [Export] private AtlasTexture _coin30;

  [ExportGroup("Meta")] [Export] public Sprite2D Sprite;

  public override void _Ready()
  {
    SetTexture();
  }
  
  public Tuple<PurseContent, int> Pickup()
  {
    QueueFree();
    return new Tuple<PurseContent, int>(PurseContent.Coin, _count);
  }

  private void SetTexture()
  {
    switch (_count)
    {
      case 1:
        Sprite.Texture = _coin1;
        return;
      case 20:
        Sprite.Texture = _coin20;
        return;
      case 30:
        Sprite.Texture = _coin30;
        return;
    }

    if (_count > 10)
    {
      GD.PushError($"Tried to spawn coin with count {_count}!");
      return;
    }
    
    // Calculate which to use
    var texture = new AtlasTexture();
    texture.Atlas = _coin1.Atlas;
    if (_count < 6)
    {
      texture.Region = _coin1.Region with
      {
        Position = new Vector2(_coin1.Region.Position.X + ((_count - 1) * 16), _coin1.Region.Position.Y)
      };
    }
    else
    {
      texture.Region = _coin1.Region with
      {
        Position = new Vector2(_coin1.Region.Position.X + ((_count - 6) * 16), _coin1.Region.Position.Y + 16)
      };
    }

    Sprite.Texture = texture;
  }
}
