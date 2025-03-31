using Flamme.common.constant;
using Flamme.common.helpers;
using Flamme.entities;
using Flamme.world.rooms;
using Godot;
using System.Collections.Generic;

namespace Flamme.projectiles.player;

public abstract partial class PlayerProjectile : Area2D
{
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public Line2D TrailLine;
  [Export] public GpuParticles2D DestructionParticles;
  
  public Vector2 Direction = Vector2.Up;
  
  protected bool Fired;
  protected bool Destructing;
  protected bool Dissipating;
  protected bool HitSomething;

  // 1:1 stats from the player, can be interpreted however
  protected float StatDamage;
  protected float StatShotSpeed;
  protected float StatRange;
  protected float FireRate;

  protected bool DestructOnHit = true;
  
  public override void _Ready()
  {
    // ExportMetaNonNull.Check(this);
    
    Direction = Direction.Normalized();
    Sprite.Visible = false;
  }

  // Counts how often shot (while shooting active)
  protected static int Counter;
  public static void ResetCounter()
  {
    Counter = 0;
  }

  public virtual void Fire(entities.player.PlayableCharacter player, Room room)
  {
    // TODO 3 Make range how far or how much time a bullet has?
    
    FireInit(player);
    CustomFireExec(player, room);
    FireReady();
  }

  private void FireInit(entities.player.PlayableCharacter player)
  {
    StatDamage = player.Stats.Damage;
    StatShotSpeed = player.Stats.ShotSpeed;
    StatRange = player.Stats.Range;
    FireRate = player.Stats.FireRate;
  }

  protected abstract void CustomFireExec(entities.player.PlayableCharacter player, Room room);

  private void FireReady()
  {
    Counter++;
    BodyEntered += OnBulletEntered;
    Fired = true;
  }
  
  protected virtual void OnBulletEntered(Node2D body)
  {
    // Change to counter for piercing rounds
    if (HitSomething)
    {
      return;
    }
    HitSomething = true;

    if (body is IPlayerDamageable enemy)
    {
      OnBulletHitEnemy(body, enemy);
    }

    if (DestructOnHit)
    {
      DestructBullet();
    }
  }
  
  protected virtual void OnBulletHitEnemy(Node2D body, IPlayerDamageable enemy)
  {
    Hit(body, enemy);
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if (!Fired)
      return;
    
    // Does more or less the trick
    Position += Direction * StatShotSpeed;
  }
  
  protected void DestructBulletInit()
  {
    if (Destructing)
      return;
    Destructing = true;
    DestructBullet();
  }

  /// <summary>
  /// Called when bullet hits something
  /// </summary>
  protected virtual void DestructBullet()
  {
    Sprite.Visible = false;
    TrailLine.Visible = false;

    if (DestructionParticles != null)
    {
      DestructionParticles.Emitting = true;
      DestructionParticles.Finished += QueueFree;
    }
    else
    {
      QueueFree();
    }
  }

  private void DissipateBulletInit()
  {
    if (Destructing)
      return;
    if (Dissipating)
      return;
    Dissipating = true;
    DissipateBullet();
  }

  /// <summary>
  /// Called when bullet has reached it's target position or range limit was reached
  /// </summary>
  protected virtual void DissipateBullet()
  {
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.TweenCallback(Callable.From(QueueFree));
  }

  protected void Hit(Node2D body, IPlayerDamageable enemy, bool knockback = true)
  {
    if (DebugToggles.InstaKill)
    {
      enemy.Hit(9999999, 0, (body.GlobalPosition - GlobalPosition).Normalized());
      return;
    }

    if (knockback)
    {
      enemy.Hit(StatDamage, StatDamage * StatShotSpeed * 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    else
    {
      enemy.Hit(StatDamage, 0, (body.GlobalPosition - GlobalPosition).Normalized());
    }
  }
}