using System;
using Flamme.common.enums;
using Godot;

namespace Flamme.entities.env.purse;

public partial class Crystal : PursePickup
{
  [Export] private int _count = 1;
  [ExportGroup("Textures")] [Export] private AtlasTexture _crystal1;

  [ExportGroup("Meta")] [Export] public Sprite2D Sprite;

  public override void _Ready()
  {
    SetTexture();
  }
  
  public override Tuple<PurseContent, int> Pickup()
  {
    QueueFree();
    return new Tuple<PurseContent, int>(PurseContent.Crystal, _count);
  }

  private void SetTexture()
  {
    if (_count == 1)
    {
      Sprite.Texture = _crystal1;
      return;
    }
    else if (_count > 3)
    {
      GD.PushError($"Tried to spawn crystal with count {_count}!");
      return;
    }
    
    var texture = new AtlasTexture();
    texture.Atlas = _crystal1.Atlas;
    texture.Region = _crystal1.Region with
    {
      Position = new Vector2(_crystal1.Region.Position.X + ((_count - 1) * 16), _crystal1.Region.Position.Y)
    };
    Sprite.Texture = texture;
  }
}