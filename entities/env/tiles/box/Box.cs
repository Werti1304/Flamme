using Godot;
using System;
using Flamme.entities;
using Flamme.entities.common;
using Flamme.testing;

public partial class Box : StaticBody2D, IPlayerDamageable, IEnemyDamagable
{
  [Export] public float StartHealth = 10;
  private float _health;
  
  [ExportGroup("Textures")]
  [Export] public Texture2D Texture1;
  [Export] public Texture2D Texture2;
  [Export] public Texture2D Texture3;
  [Export] public Texture2D TextureBroken;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public CollisionShape2D CollisionShape;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    _health = StartHealth;

    Update();
  }

  public float GetHealth()
  {
    return _health;
  }

  public void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    _health -= attackDamage;

    Update();
  }

  private void Update()
  {
    var percent = _health / StartHealth;
    if (percent >= 0.66f)
    {
      Sprite.Texture = Texture1;
    }
    else if (percent >= 0.33f)
    {
      Sprite.Texture = Texture2;
    }
    else if (percent > 0f)
    {
      Sprite.Texture = Texture3;
    }
    else
    {
      Sprite.Texture = TextureBroken;
      CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }
  }

  public void TakeDamage(int damage)
  {
    Hit(damage, 0, Vector2.Zero);
  }
}
