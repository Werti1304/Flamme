using Flamme.entities;
using Flamme.entities.player;
using Godot;

namespace Flamme.testing;

public partial class Bullet : Area2D
{
  [Export] public Vector2 Direction = Vector2.Down;

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;

  private PlayerStats _playerStats;
  private bool _fired = false;
  private bool _destructing = false;
  private bool _hitSomething = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    Sprite.Visible = false;
  }

  public void Fire(PlayerStats playerStats)
  {
    BodyEntered += OnBulletEntered;

    _playerStats = playerStats;
    var timeTillDisappear = _playerStats.Range / _playerStats.ShotSpeed;
    GetTree().CreateTimer(timeTillDisappear).Timeout += InitBulletDestruction;
    _fired = true;
    Sprite.Visible = true;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!_fired)
      return;
    
    // Does more or less the trick
    Position += Direction * _playerStats.ShotSpeed;
  }

  private void OnBulletEntered(Node2D body)
  {
    // Change to counter for piercing rounds
    if (_hitSomething)
    {
      return;
    }
    _hitSomething = true;
      
    if (body is Door door)
    {
      door.Open();
    }

    if (body is IPlayerDamageable enemy)
    {
      enemy.Hit(_playerStats.Damage, 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    QueueFree();
  }

  private void InitBulletDestruction()
  {
    if (_destructing)
      return;
    
    _destructing = true;
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, Node2D.PropertyName.Scale.ToString(), Vector2.Zero, 0.1f);
    tween.TweenCallback(Callable.From(QueueFree));
  }
}