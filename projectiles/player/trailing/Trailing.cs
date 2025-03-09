using Flamme;
using Flamme.entities;
using Flamme.entities.player;
using Flamme.testing;
using Godot;
using System;
using System.Collections.Generic;

public partial class Trailing : Area2D
{
  [Export] public Vector2 Direction = Vector2.Up;

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public Line2D TrailLine;
  [Export] public GpuParticles2D DestructionParticles;

  // TODO 1 Perf maybe replace with simpler types
  private PlayerStats _playerStats;
  
  private bool _fired = false;
  private bool _destructing = false;
  private bool _hitSomething = false;
  
  private Vector2 _startPointP0;
  private Vector2 _controlPointP1;
  private Vector2 _endPointP2;
  
  private Vector2 normalToDirection = Vector2.Left;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    Sprite.Visible = false;
  }

  // Counts how often shot (while shooting active)
  public static int Counter = 0;

  public void Fire(PlayerStats playerStats)
  {
    BodyEntered += OnBulletEntered;

    Sprite.Modulate = Colors.Transparent;
    TrailLine.Modulate = Colors.Transparent;

    _playerStats = playerStats;

    var rangeInPx = _playerStats.Range * 32;
    
    Direction = Direction.Normalized(); // Just to make sure
    normalToDirection = new Vector2(-Direction.Y, Direction.X).Normalized();
    
    // var timeTillDisappear = _playerStats.Range / _playerStats.ShotSpeed;
    // GetTree().CreateTimer(timeTillDisappear).Timeout += InitBulletDestruction;
    
    
    // Calculate points for bezier curve
    
    TrailLine.ClearPoints();
    
    // Determine magnitude of swing
    // In this case this means it's +- 16 to 32 pixels
    var magnitude = Main.Instance.Rnd.RandfRange(-4.0f, 4.0f);;
    _startPointP0 = GlobalPosition + normalToDirection * magnitude / 4.0f;

    if (Counter % 3 == 0)
    {
      magnitude -= 16.0f ;
    }
    else if (Counter % 3 == 1)
    {
      magnitude += 16.0f;
    }
    // else no magnitude

    Counter++;
    
    _endPointP2 = GlobalPosition + Direction * rangeInPx;
    _controlPointP1 = GlobalPosition + Direction * rangeInPx / 2 + normalToDirection * magnitude;
    
    TrailLine.AddPoint(_startPointP0);
    // TrailLine.AddPoint(_controlPointP1);
    // TrailLine.AddPoint(_endPointP2);
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.White, 0.1f);
    _fired = true;
  }

  private Queue<(double, Vector2)> _points = new Queue<(double, Vector2)>();
  

  double t = 0;
  public override void _Process(double delta)
  {
    if (!_fired || _hitSomething)
      return;
    
    t += delta;
    GlobalPosition = QuadraticBezier((float)t);

    if (Sprite.Visible == false)
    {
      Sprite.Visible = true;
    }
    
    TrailLine.AddPoint(GlobalPosition);
    _points.Enqueue((t, GlobalPosition));

    if (_points.Count > 0)
    {
      for (;;)
      {
        var point = _points.Peek();
        if (t > point.Item1 + 0.3f)
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

    if (t >= 1)
    {
      InitBulletDestruction();
    }
    
    //
    // if (t >= 100)
    // {
    //   QueueFree();
    // }
  }

  private void OnBulletEntered(Node2D body)
  {
    // Change to counter for piercing rounds
    if (_hitSomething)
    {
      return;
    }
    _hitSomething = true;

    if (body is IPlayerDamageable enemy)
    {
      enemy.Hit(_playerStats.Damage, 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    DestructBullet();
  }

  private void InitBulletDestruction()
  {
    if (_destructing)
      return;
    
    _destructing = true;
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.Parallel().TweenProperty(TrailLine, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 0.2f);
    tween.TweenCallback(Callable.From(QueueFree));
  }

  private void DestructBullet()
  {
    Sprite.Visible = false;
    TrailLine.Visible = false;
    DestructionParticles.Emitting = true;
    DestructionParticles.Finished += QueueFree;
  }
  
  // https://docs.godotengine.org/en/latest/tutorials/math/beziers_and_curves.html#quadratic-bezier
  private Vector2 QuadraticBezier(float t)
  {
    var q0 = _startPointP0.Lerp(_controlPointP1, t);
    var q1 = _controlPointP1.Lerp(_endPointP2, t);
    
    var r = q0.Lerp(q1, t);
    return r;
  }
  
  private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
  {
    var q0 = p0.Lerp(p1, t);
    var q1 = p1.Lerp(p2, t);
    var q2 = p2.Lerp(p3, t);

    var r0 = q0.Lerp(q1, t);
    var r1 = q1.Lerp(q2, t);

    var s = r0.Lerp(r1, t);
    return s;
  }
}
