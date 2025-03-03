using Flamme.entities;
using Godot;
using System;

namespace Flamme.testing;

public partial class Enemy : CharacterBody2D, IPlayerDamageable
{
  [Export] public int Speed = 100;
  [Export] public int MaxSpeed = 100;
  [Export] public float Health = 10;
  [Export] public int MeleeDamage = 1;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public Timer StunTimer;
  [Export] public AtlasTexture RunRightTexture;
  [Export] public AtlasTexture RunLeftTexture;

  public CharacterBody2D Chasing = null;

  private bool _stunned = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    StunTimer.Timeout += () => _stunned = false;
    
    throw new StackOverflowException("Don't use me");
  }

  public int GetMeleeDamage()
  {
    return MeleeDamage;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (Chasing == null)
    {
      return;
    }
    
    for (var i = 0; i < GetSlideCollisionCount(); i++)
    {
      var collider = GetSlideCollision(i);
      
      if (collider.GetCollider() is RigidBody2D rigidBody2D)
      {
        rigidBody2D.ApplyCentralForce(-collider.GetNormal() * 100);
      }
      if (collider.GetCollider() is Enemy e)
      {
        e.Velocity += -collider.GetNormal() * 50;
      }
    }

    if (!_stunned)
    {
      var direction = (Chasing.GlobalPosition - GlobalPosition).Normalized();
      Sprite.Texture = direction.X > 0 ? RunRightTexture : RunLeftTexture;
      Velocity += Speed * direction;
      Velocity = Velocity.LimitLength(MaxSpeed);
    }
    
    MoveAndSlide();
  }

  public float GetHealth()
  {
    return Health;
  }

  public void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    Health -= attackDamage;
    _stunned = true;

    if (Health < 1)
    {
      QueueFree();
    }
    
    Velocity += knockBackStrength * attackDirection * (1 / Scale.X);
    StunTimer.Start();
  }
}