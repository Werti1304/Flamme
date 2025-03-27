using Flamme.entities;
using Flamme.entities.player;
using Flamme.world.rooms;
using Godot;
using System.Collections.Generic;

namespace Flamme.projectiles.player.trailing;

public partial class Trailing : PlayerProjectile
{
  [ExportGroup("Textures")]
  [Export] public Texture2D TrailingTexture;
  [Export] public Texture2D HomingTexture;
  
  private Vector2 _startPointP0;
  private Vector2 _controlPointP1;
  private Vector2 _endPointP2;
  
  private Vector2 _normalToDirection = Vector2.Left;
  
  private entities.enemies.Enemy _homingTarget = null;
  private bool _homing = false;

  protected override void CustomFireExec(entities.player.PlayableCharacter player, Room room)
  {
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

    if (Counter % 3 == 0)
    {
      magnitude -= 16.0f ;
    }
    else if (Counter % 3 == 1)
    {
      magnitude += 16.0f;
    }
    // else no magnitude

    
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
        entities.enemies.Enemy nearestEnemy = null;
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
      Sprite.Texture = HomingTexture;
    }
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
  }

  private readonly Queue<(double, Vector2)> _points = new Queue<(double, Vector2)>();
  private double _t = 0;
  
  public override void _Process(double delta)
  {
    if (!Fired || HitSomething)
      return;

    if (_homing && IsInstanceValid(_homingTarget))
    {
      _endPointP2 = _homingTarget.GlobalPosition;
    }
    
    _t += delta;
    GlobalPosition = ProjectileHelper.QuadraticBezier(_startPointP0, _controlPointP1, _endPointP2, (float)_t);

    if (Sprite.Visible == false)
    {
      Sprite.Visible = true;
    }
    
    TrailLine.AddPoint(GlobalPosition);
    _points.Enqueue((_t, GlobalPosition));

    if (_points.Count > 0)
    {
      for (;;)
      {
        var point = _points.Peek();
        if (_t > point.Item1 + 0.3f)
        {
          TrailLine.RemovePoint(0);
          _points.Dequeue();
        }
        else
        {
          break;
        }
      }
    }

    if (_t >= 1)
    {
      DissipateBullet();
    }
  }
}