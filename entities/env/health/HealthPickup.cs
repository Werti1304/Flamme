using System;
using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.testing;
using Godot;

namespace Flamme.entities.env.health;

public partial class HealthPickup : Area2D
{
  [Export] public HealthType HealthType = HealthType.Normal;
  [Export] public int HealingAmount = 4;
  
  [ExportGroup("Textures")] 
  [Export] public Texture2D HeartFull;
  [Export] public Texture2D Heart3Qt;
  [Export] public Texture2D HeartHalf;
  [Export] public Texture2D Heart1Qt;
  [Export] public Texture2D AbsorptionHeartFull;
  [Export] public Texture2D AbsorptionHeart3Qt;
  [Export] public Texture2D AbsorptionHeartHalf;
  [Export] public Texture2D AbsorptionHeart1Qt;

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    if (HealingAmount is <= 0 or > Universal.HealthPerContainer)
    {
      GD.PushError($"Tried to spawn a heart pickup with {HealingAmount} healing.");
      return;
    }

    Sprite.Texture = HealthType switch
    {
      HealthType.Normal => HealingAmount switch
      {
        4 => HeartFull,
        3 => Heart3Qt,
        2 => HeartHalf,
        1 => Heart1Qt,
        _ => null
      },
      HealthType.Absorption => HealingAmount switch
      {
        4 => AbsorptionHeartFull,
        3 => AbsorptionHeart3Qt,
        2 => AbsorptionHeartHalf,
        1 => AbsorptionHeart1Qt,
        _ => null
      },
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public void Consumed()
  {
    QueueFree();
  }
}