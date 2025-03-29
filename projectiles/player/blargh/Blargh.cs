using Flamme;
using Flamme.common.constant;
using Flamme.entities;
using Flamme.entities.enemies;
using Flamme.entities.player;
using Flamme.projectiles;
using Flamme.projectiles.player;
using Flamme.world.rooms;
using Godot;
using System;
using System.Collections.Generic;

public partial class Blargh : PlayerProjectile
{
  [ExportGroup("Meta")]
  [Export] public CollisionShape2D CollisionShape;
  
  private Vector2 _startPointP0;
  private Vector2 _controlPointP1;
  private Vector2 _endPointP2;
  
  private Vector2 _normalToDirection = Vector2.Left;
  
  private Enemy _homingTarget;
  private bool _homing;
  
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
    _startPointP0 = GlobalPosition + _normalToDirection * magnitude / 4.0f;
    
    _endPointP2 = GlobalPosition + Direction * rangeInPx;
    _controlPointP1 = GlobalPosition + Direction * rangeInPx / 2 + _normalToDirection * magnitude;
    
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
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.TweenInterval(0.7f * totalTime);
    tween.TweenProperty(TrailLine, Line2D.PropertyName.Width.ToString(), 0, 0.2f  * totalTime ).SetTrans(Tween.TransitionType.Quint);
    tween.TweenCallback(Callable.From(DestructBulletInit));
  }
  
  protected override void OnBulletHitEnemy(Node2D body, IPlayerDamageable enemy)
  {
    if (DebugToggles.InstaKill)
    {
      enemy.Hit(9999999, StatDamage * StatShotSpeed * 100, (body.GlobalPosition - GlobalPosition).Normalized());
      return;
    }
    enemy.Hit(StatDamage, StatDamage * StatShotSpeed * 100, (body.GlobalPosition - GlobalPosition).Normalized());
  }

  private readonly Queue<(double, Vector2)> _points = new Queue<(double, Vector2)>();
  private double _t;
  public override void _Process(double delta)
  {
    if (!Fired || HitSomething)
      return;

    if (_homing && IsInstanceValid(_homingTarget))
    {
      _endPointP2 = _homingTarget.GlobalPosition;
    }
    
    _t += delta * 4.0;
    GlobalPosition = ProjectileHelper.QuadraticBezier(_startPointP0, _controlPointP1, _endPointP2, (float)_t);

    if (Sprite.Visible == false)
    {
      Sprite.Visible = true;
    }
    
    TrailLine.AddPoint(GlobalPosition);
    _points.Enqueue((_t, GlobalPosition));
    
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
