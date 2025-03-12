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
  [Export] public Line2D TrailLine;
  [Export] public GpuParticles2D DestructionParticles;
  
  public Vector2 Direction = Vector2.Up;
  
  protected bool Fired;
  protected bool Destructing;
  protected bool Dissipating;
  protected bool HitSomething;
  
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

  public virtual void Fire(Enemy enemy, Room room)
  {
    // TODO 3 Make range how far or how much time a bullet has?
    
    FireInit(enemy);
    CustomFireExec(enemy, room);
    FireReady();
  }

  private void FireInit(Enemy enemy)
  { }

  protected abstract void CustomFireExec(Enemy enemy, Room room);

  private void FireReady()
  {
    Counter++;
    BodyEntered += OnBulletEntered;
    SetPhysicsProcess(true);
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
    if (!Fired)
      return;
    
    // Does more or less the trick
    Position += Direction * ShotSpeed;
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
    TrailLine.Visible = false;
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
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.TweenCallback(Callable.From(QueueFree));
  }
}
