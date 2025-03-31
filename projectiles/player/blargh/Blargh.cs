using Flamme;
using Flamme.common.constant;
using Flamme.entities;
using Flamme.entities.enemies;
using Flamme.entities.player;
using Flamme.projectiles;
using Flamme.projectiles.player;
using Flamme.world.rooms;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Blargh : PlayerProjectile
{
  [ExportGroup("Meta")]
  [Export] public CollisionPolygon2D LineCollisionShape;
  [Export] public Area2D LineCollisionArea;
  [Export] public GpuParticles2D SpawnParticles;
  [Export] public Array<GpuParticles2D> EndParticles = new Array<GpuParticles2D>();
  
  private Vector2 _startPointP0;
  private Vector2 _controlPointP1;
  private Vector2 _endPointP2;
  
  private Vector2 _normalToDirection = Vector2.Left;
  
  private Enemy _homingTarget;
  private bool _homing;

  public override void _Ready()
  {
    BodyExited += OnBulletLeave;
    
    LineCollisionArea.BodyEntered += OnBulletEntered;
    LineCollisionArea.BodyExited += OnBulletLeave;
    
    Sprite.GlobalPosition = GlobalPosition;
    Sprite.Visible = false;

    SpawnParticles.GlobalPosition = GlobalPosition;
    SpawnParticles.Emitting = false;
    
    DestructionParticles.GlobalPosition = GlobalPosition;

    foreach (var particles in EndParticles)
    {
      particles.Emitting = false;
    }
  }

  protected override void CustomFireExec(PlayableCharacter player, Room room)
  {
    DestructOnHit = false;
    
    Sprite.Modulate = Colors.Transparent;
    TrailLine.Modulate = Colors.Transparent;
    
    var rangeInPx = StatRange * 32;
    
    Direction = Direction.Normalized(); // Just to make sure
    _normalToDirection = new Vector2(-Direction.Y, Direction.X).Normalized();
    
    TrailLine.ClearPoints();
    
    // Determine magnitude of swing
    // In this case this means it's +- 16 to 32 pixels
    var magnitude = Main.Instance.Rnd.RandfRange(-4.0f, 4.0f);;
    _startPointP0 = GlobalPosition;
    
    _endPointP2 = GlobalPosition + Direction * rangeInPx;
    _controlPointP1 = GlobalPosition + Direction * rangeInPx / 2;
    
    TrailLine.AddPoint(_startPointP0);
    
    // homing
    if (player.Modifiers.IsHoming)
    {
      if (room.Enemies.Count > 0)
      {
        // Select nearest enemy
        var nearestDistance = float.MaxValue;
        Enemy nearestEnemy = null;
        foreach (var enemy in room.Enemies)
        {
          var distance = enemy.GlobalPosition.DistanceTo(_startPointP0);
          if (distance < nearestDistance)
          {
            nearestDistance = distance;
            nearestEnemy = enemy;
          }
        }
        _homingTarget = nearestEnemy;
        _homing = true;
      }
      // Sprite.Texture = HomingTexture;
    }
    
    // We have exactly 1 / FireRate time in total
    var totalTime = 1/FireRate;
    
    // CollisionShape.Pol
    
    SpawnParticles.Lifetime = 0.5f * totalTime;
    SpawnParticles.OneShot = true;
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.TweenInterval(0.5f * totalTime);
    // Stop all end particles shortly before the trail gets smaller
    tween.TweenInterval(0.1f * totalTime);
    foreach (var particle in EndParticles)
    {
      tween.Parallel().TweenProperty(particle, GpuParticles2D.PropertyName.Emitting.ToString(), false, 0.1f * totalTime);
    }
    tween.TweenProperty(TrailLine, Line2D.PropertyName.Width.ToString(), 0, 0.3f  * totalTime ).SetTrans(Tween.TransitionType.Expo);
    tween.TweenCallback(Callable.From(DestructBulletInit));
  }

  private List<(Node2D, IPlayerDamageable)> _playerDamageables = new List<(Node2D, IPlayerDamageable)>();
  protected override void OnBulletEntered(Node2D body)
  {
    GD.Print($"Hit something: {body}");
    
    if (body is TileMapLayer tileMapLayer && tileMapLayer.Name != "Props")
    {
      HitSomething = true;

      foreach (var particle in EndParticles)
      {
        particle.Emitting = true;
      }
    }
    else if(body is IPlayerDamageable damageable)
    {
      _playerDamageables.Add((body, damageable));
    }

    if (body is IPlayerDamageable enemy)
    {
      OnBulletHitEnemy(body, enemy);
    }
  }

  private void OnBulletLeave(Node2D body)
  {
    if (body is IPlayerDamageable damageable)
    {
      _playerDamageables.Remove((body, damageable));
    }
  }

  protected override void OnBulletHitEnemy(Node2D body, IPlayerDamageable enemy)
  {
    Hit(body, enemy);
  }

  private readonly Queue<(double, Vector2)> _points = new Queue<(double, Vector2)>();
  private double _t;
  private int _tickCounter = 0;
  public override void _PhysicsProcess(double delta)
  {
    if (!Fired)
      return;
    
    if (_playerDamageables.Count > 0 && _tickCounter % 30 == 0) // Every 30 ticks, try to damage
    {
      foreach (var damageable in _playerDamageables)
      {
        Hit(damageable.Item1, damageable.Item2);
      }
    }

    if (HitSomething)
      return;

    if (_homing && IsInstanceValid(_homingTarget))
    {
      _endPointP2 = _homingTarget.GlobalPosition;
    }
    
    var oldPosition = GlobalPosition;
    _t += delta * 4.0;
    GlobalPosition = ProjectileHelper.QuadraticBezier(_startPointP0, _controlPointP1, _endPointP2, (float)_t);

    var direction = GlobalPosition - oldPosition;
    var normalDirection = new Vector2(-Direction.Y, Direction.X).Normalized();
    
    if (_tickCounter < 3) // For the first 3 ticks, correct the rotation
    {
      var angle = (GlobalPosition - oldPosition).Angle();
      Sprite.Visible = true;
      Sprite.Rotation = angle;
      SpawnParticles.Rotation = angle;
      SpawnParticles.Emitting = true;
    }
    
    TrailLine.AddPoint(GlobalPosition);
    _points.Enqueue((_t, GlobalPosition));

    if (_tickCounter % 4 == 0)
    {
      // All this so the polygon gets calculated correctly
      var half = LineCollisionShape.Polygon.Length / 2;
      var newPolygon = new List<Vector2>();
      newPolygon.AddRange(LineCollisionShape.Polygon);
      // -> Line Width / 2
      newPolygon.Insert(half, GlobalPosition + normalDirection * 11);
      newPolygon.Insert(half, GlobalPosition - normalDirection * 11);
      LineCollisionShape.Polygon = newPolygon.ToArray();
      
      GD.Print($"Tick {_tickCounter}");
      foreach (var vector in LineCollisionShape.Polygon)
      {
        GD.Print($"Vector: {vector}");
      }
    }

    _tickCounter++;

    // TODO 4 Change collision shape approprietly
    // var shape = CollisionShape.Shape as RectangleShape2D;
    // CollisionShape.Shape
    // SetDeferred(CollisionShape);

    // if (_points.Count > 0)
    // {
    //   for (;;)
    //   {
    //     var point = _points.Peek();
    //     if (_t > point.Item1 + 0.2f)
    //     {
    //       TrailLine.RemovePoint(0);
    //       _points.Dequeue();
    //     }
    //     else
    //     {
    //       break;
    //     }
    //   }
    // }

    // if (_t >= 1)
    // {
    //   DissipateBullet();
    // }
  }
}
