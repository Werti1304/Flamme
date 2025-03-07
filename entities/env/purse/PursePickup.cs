using System;
using Flamme.common.enums;
using Godot;

namespace Flamme.entities.env.purse;

public partial class PursePickup : RigidBody2D
{
  [Export] public PurseContent PurseContent;
  [Export] public int Amount = 1;
  
  [ExportGroup("Textures")] 
  [Export] private AtlasTexture _coin1;
  [Export] private AtlasTexture _coin20;
  [Export] private AtlasTexture _coin30;
  [Export] private AtlasTexture _crystal1;
  [Export] private AtlasTexture _key;

  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;

  public override void _Ready()
  {
    switch (PurseContent)
    {
      case PurseContent.Coin:
        SetCoinTexture();
        break;
      case PurseContent.Crystal:
        SetCrystalTexture();
        break;
      case PurseContent.Key:
        Sprite.Texture = _key;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void SetCoinTexture()
  {
    switch (Amount)
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

    if (Amount > 10)
    {
      GD.PushError($"Tried to spawn coin with count {Amount}!");
      return;
    }
    
    // Calculate which to use
    var texture = new AtlasTexture();
    texture.Atlas = _coin1.Atlas;
    if (Amount < 6)
    {
      texture.Region = _coin1.Region with
      {
        Position = new Vector2(_coin1.Region.Position.X + ((Amount - 1) * 16), _coin1.Region.Position.Y)
      };
    }
    else
    {
      texture.Region = _coin1.Region with
      {
        Position = new Vector2(_coin1.Region.Position.X + ((Amount - 6) * 16), _coin1.Region.Position.Y + 16)
      };
    }

    Sprite.Texture = texture;
  }
  
  private void SetCrystalTexture()
  {
    if (Amount == 1)
    {
      Sprite.Texture = _crystal1;
      return;
    }
    else if (Amount > 3)
    {
      GD.PushError($"Tried to spawn crystal with count {Amount}!");
      return;
    }
    
    var texture = new AtlasTexture();
    texture.Atlas = _crystal1.Atlas;
    texture.Region = _crystal1.Region with
    {
      Position = new Vector2(_crystal1.Region.Position.X + ((Amount - 1) * 16), _crystal1.Region.Position.Y)
    };
    Sprite.Texture = texture;
  }

  public Tuple<PurseContent, int> Pickup()
  {
    QueueFree();
    return new Tuple<PurseContent, int>(PurseContent, Amount);
  }
}