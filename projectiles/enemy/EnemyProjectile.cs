using Flamme.entities.common;
using Flamme.testing;
using Flamme.world.rooms;
using Godot;
using System;

public abstract partial class EnemyProjectile : Area2D
{
  [Export] public int Damage = 1;
  [Export] public float ShotSpeed = 10.0f;
  
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public GpuParticles2D DestructionParticles;
  
  public Vector2 Direction = Vector2.Up;
  
  protected bool Fired;
  protected bool Destructing;
  protected bool Dissipating;
  protected bool HitSomething;

  private Enemy _shooter;
  private float _range;
  // TODO Make ShootingEnemy class?
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    Sprite.ZIndex = 10;
    DestructionParticles.ZIndex = 2000;
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

  public virtual void Fire(Enemy enemy, Room room, float range)
  {
    // TODO 3 Make range how far or how much time a bullet has?
    
    FireInit(enemy, range);
    CustomFireExec(enemy, room);
    FireReady();
  }

  private void FireInit(Enemy enemy, float range)
  {
    _shooter = enemy;
    _range = range;
  }

  protected abstract void CustomFireExec(Enemy enemy, Room room);

  private void FireReady()
  {
    Counter++;
    BodyEntered += OnBulletEntered;
    SetPhysicsProcess(true);
    Sprite.Visible = true;
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
    DestructBullet();
  }

  protected virtual void OnBulletHit(Node2D body, IEnemyDamagable player)
  {
    player.TakeDamage(Damage);
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if (!Fired || HitSomething)
      return;
    
    // Does more or less the trick
    Position += Direction * ShotSpeed;

    if (Position.DistanceTo(_shooter.GlobalPosition) > _range)
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
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.1f);
    tween.TweenCallback(Callable.From(QueueFree));
  }
}
