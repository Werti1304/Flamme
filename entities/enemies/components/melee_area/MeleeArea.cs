using Flamme.entities.common;
using Godot;
using System.Collections.Generic;

namespace Flamme.entities.enemies.components.melee_area;

public partial class MeleeArea : Area2D
{
  [Signal] public delegate void DamagedTargetEventHandler(int damage);
  
  [Export] public bool MeleeDamageEnabled = true;
  [Export] public int Damage = 1;

  private readonly List<IEnemyDamagable> _damagables = [];
  
  private const double DamageMaxTime = 0.1;  
  private double _damageTimer = 0;
  
  public override void _Ready()
  {
    BodyEntered += OnBodyEntered;
    BodyExited += OnBodyExited;
    
    // Is only activated after area is entered
    SetPhysicsProcess(false);
  }
  
  private void OnBodyEntered(Node2D body)
  {
    if (!MeleeDamageEnabled)
      return;
    
    if (body is IEnemyDamagable e)
    {
      if (!IsPhysicsProcessing())
      {
        SetPhysicsProcess(true);
      }
      _damagables.Add(e);
      e.TakeDamage(Damage);
    }
  }
  
  private void OnBodyExited(Node2D body)
  {
    if (!MeleeDamageEnabled)
      return;
    
    if (body is IEnemyDamagable e)
    {
      _damagables.Remove(e);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!MeleeDamageEnabled || _damagables.Count == 0)
    {
      SetPhysicsProcess(false);
      return;
    }
    
    _damageTimer += delta;

    // This timer is very short cuz invincibility frames should be be done on the players / damagables side
    if (!(_damageTimer > DamageMaxTime))
    {
      return;
    }
    
    _damageTimer = 0;

    foreach (var damagable in _damagables)
    {
      if (damagable.TakeDamage(Damage))
      {
        EmitSignal(SignalName.DamagedTarget, Damage);
      }
    }
  }
}