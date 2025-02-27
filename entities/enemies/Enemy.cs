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
  [Export] public float Health = 10;

  [ExportGroup("Melee Damage")] 
  [Export] public bool DoesMeleeDamage = true;
  [Export] public int MeleeDamage = 1;
  // Tries to do Melee Damage every MeleeDmgTime [s]
  private const double MeleeDmgTime = 0.1;
  // Every damagable currently in melee range
  private readonly List<IEnemyDamagable> _damagables = new List<IEnemyDamagable>();
  // This is just to not check every (physics) frame
  private double _damagableTimer = 0;

  [ExportGroup("Meta")] 
  [Export] protected Area2D EnemyArea;

  [Signal]
  public delegate void DeathEventHandler();

  protected PlayableCharacter Target;
  private Room _room;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    Death += OnDeath;
    
    EnemyArea.BodyEntered += EnemyAreaOnBodyEntered;
    EnemyArea.BodyExited += EnemyAreaOnBodyExited;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!DoesMeleeDamage || _damagables.Count == 0)
      return;

    _damagableTimer += delta;

    // This timer is very short cuz invincibility frames should be be done on the players / damagables side
    if (!(_damagableTimer > MeleeDmgTime))
    {
      return;
    }
    
    _damagableTimer = 0;

    foreach (var damagable in _damagables)
    {
      damagable.TakeDamage(MeleeDamage);
    }
  }

  public abstract void OnDeath();
  
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
    EmitSignal(SignalName.Death);
    return true;
  }

  public virtual void SetActive(PlayableCharacter playableCharacter)
  {
    GD.Print($"Enemy {Name} active!");
    Target = playableCharacter;
  }
  
  public virtual void SetPassive()
  {
    GD.Print($"Enemy {Name} passive again.");
    Target = null;
  }
  
  private void EnemyAreaOnBodyEntered(Node2D body)
  {
    if (!DoesMeleeDamage)
      return;
    
    if (body is IEnemyDamagable e)
    {
      e.TakeDamage(MeleeDamage);
      _damagables.Add(e);
    }
  }
  
  private void EnemyAreaOnBodyExited(Node2D body)
  {
    if (!DoesMeleeDamage)
      return;
    
    if (body is IEnemyDamagable e)
    {
      _damagables.Remove(e);
    }
  }

  public float GetHealth()
  {
    return Health;
  }

  public virtual void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    TakeDamage(attackDamage);
  }
}
