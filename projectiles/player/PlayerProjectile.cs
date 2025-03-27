using Flamme.common.constant;
using Flamme.common.helpers;
using Flamme.entities;
using Flamme.world.rooms;
using Godot;

namespace Flamme.projectiles.player;

public abstract partial class PlayerProjectile : Area2D
{
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public Line2D TrailLine;
  [Export] public GpuParticles2D DestructionParticles;
  
  public Vector2 Direction = Vector2.Up;
  
  protected bool Fired = false;
  protected bool Destructing = false;
  protected bool Dissipating = false;
  protected bool HitSomething = false;

  // 1:1 stats from the player, can be interpreted however
  protected float StatDamage = 0;
  protected float StatShotSpeed = 0;
  protected float StatRange = 0;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    Direction = Direction.Normalized();
    Sprite.Visible = false;
  }

  // Counts how often shot (while shooting active)
  protected static int Counter = 0;
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
  }

  protected abstract void CustomFireExec(entities.player.PlayableCharacter player, Room room);

  private void FireReady()
  {
    Counter++;
    BodyEntered += OnBulletEntered;
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

    if (body is IPlayerDamageable enemy)
    {
      OnBulletHit(body, enemy);
    }
    DestructBullet();
  }

  protected virtual void OnBulletHit(Node2D body, IPlayerDamageable enemy)
  {
    if (DebugToggles.InstaKill)
    {
      enemy.Hit(9999999, StatDamage * StatShotSpeed * 100, (body.GlobalPosition - GlobalPosition).Normalized());
      return;
    }
#pragma warning disable CS0162 // Unreachable code detected
    enemy.Hit(StatDamage, StatDamage * StatShotSpeed * 100, (body.GlobalPosition - GlobalPosition).Normalized());
#pragma warning restore CS0162 // Unreachable code detected
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if (!Fired)
      return;
    
    // Does more or less the trick
    Position += Direction * StatShotSpeed;
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