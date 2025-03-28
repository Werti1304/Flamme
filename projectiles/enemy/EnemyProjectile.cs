using Flamme.common.helpers;
using Flamme.entities.common;
using Flamme.world.rooms;
using Godot;

namespace Flamme.projectiles.enemy;

public abstract partial class EnemyProjectile : Area2D
{
  [Export] public int Damage = 1;
  [Export] public float ShotSpeed = 10.0f;

  [ExportGroup("Effects")] 
  [Export] public float WindupTime;
  [Export] public float DissipateTime = 0.1f;
  
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public GpuParticles2D DestructionParticles;
  
  public Vector2 Direction = Vector2.Up;
  
  protected bool Fired;
  protected bool Destructing;
  protected bool Dissipating;
  protected bool HitSomething;

  protected entities.enemies.Enemy Shooter;
  protected float Range;
  protected entities.player.PlayableCharacter Target;
  // TODO Make ShootingEnemy class?
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    SetPhysicsProcess(false);
    Direction = Direction.Normalized();
    Sprite.Visible = false;
  }

  // Counts how often shot (while shooting active)
  protected static int Counter;
  public static void ResetCounter()
  {
    Counter = 0;
  }

  public virtual void Fire(entities.enemies.Enemy enemy, Room room, entities.player.PlayableCharacter target, float range)
  {
    FireInit(enemy, range, target);
    CustomFireExec(enemy, room);
    if (WindupTime > 0.01f)
    {
      var tween = GetTree().CreateTween();
      Sprite.Modulate = Colors.Transparent;
      Sprite.Show();
      tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, WindupTime);
      tween.TweenCallback(Callable.From(FireReady));
    }
    else
    {
      FireReady();
    }
  }

  private void FireInit(entities.enemies.Enemy enemy, float range, entities.player.PlayableCharacter playableCharacter)
  {
    Shooter = enemy;
    Range = range;
    Target = playableCharacter;
  }

  protected virtual void CustomFireExec(entities.enemies.Enemy enemy, Room room)
  { }

  private void FireReady()
  {
    Counter++;
    BodyEntered += OnBulletEntered;
    SetPhysicsProcess(true);
    Sprite.Visible = true;
    _lastPosition = GlobalPosition;
    Fired = true;
  }
  
  private void OnBulletEntered(Node2D body)
  {
    // Change to counter for piercing rounds
    if (HitSomething)
    {
      return;
    }
    HitSomething = true;

    if (body is IEnemyDamagable player)
    {
      OnBulletHit(body, player);
    }
    DestructBulletInit();
  }

  protected virtual void OnBulletHit(Node2D body, IEnemyDamagable player)
  {
    player.TakeDamage(Damage);
  }

  private double _distanceTraveled;
  private Vector2 _lastPosition;
  public override void _PhysicsProcess(double delta)
  {
    if (!Fired || HitSomething)
      return;
    
    // Does more or less the trick
    
    Position += Direction * ShotSpeed;
    
    _distanceTraveled += GlobalPosition.DistanceTo(_lastPosition);
    _lastPosition = GlobalPosition;
    if (_distanceTraveled > Range)
    {
      DissipateBulletInit(); 
    }
  }
  
  private void DestructBulletInit()
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
    DestructionParticles.Emitting = true;
    DestructionParticles.Finished += QueueFree;
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
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, DissipateTime);
    tween.TweenCallback(Callable.From(QueueFree));
  }
}