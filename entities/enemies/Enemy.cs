using Flamme.entities;
using Flamme.entities.common;
using Flamme.testing;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Room = Flamme.world.rooms.Room;

public abstract partial class Enemy : CharacterBody2D, IPlayerDamageable
{
  private float _health = 10;
  [Export] public bool EnemyDisabled = false;
  [Export]   public float Health
  {
    get => _health;
    set
    {
      _health = value;
      EmitSignal(SignalName.HealthChanged, this);
    } 
  }
  [Export] public float Weight = 10.0f;
  
  [Signal] public delegate void HealthChangedEventHandler(Enemy enemy);

  public float MaxHealth;
  
  protected bool IsActive => Target != null;

  protected PlayableCharacter Target;
  private Room _room;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    SetPhysicsProcess(false);
    
    MaxHealth = Health;
  }

  public virtual void OnDeath()
  {
    QueueFree(); 
  }
  
  /// <summary>
  /// When the enemy takes damage
  /// </summary>
  /// <param name="attackDamage">How much damage</param>
  /// <returns>Whether the enemy died (true)</returns>
  public bool TakeDamage(float attackDamage)
  {
    Health -= attackDamage;

    if (Health > 0)
    {
      return false;
    }
    Health = 0;
    OnDeath();
    return true;
  }

  public virtual void SetActive(PlayableCharacter playableCharacter)
  {
    if (EnemyDisabled)
    {
      return;
    }
    GD.Print($"Enemy {Name} active!");
    SetPhysicsProcess(true);
    Target = playableCharacter;
    OnSetActive();
  }
  
  protected virtual void OnSetActive()
  { }
  
  public virtual void SetPassive()
  {
    GD.Print($"Enemy {Name} passive again.");
    SetPhysicsProcess(false);
    OnSetPassive();
    Target = null;
  }
  
  protected virtual void OnSetPassive()
  { }

  public float GetHealth()
  {
    return Health;
  }


  protected virtual void OnHit()
  { }

  public virtual void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    OnHit();
    TakeDamage(attackDamage);
    Velocity += (knockBackStrength / Weight) * attackDirection;
  }
}
