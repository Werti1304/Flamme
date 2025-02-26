using Flamme.entities;
using Flamme.entities.player;
using Godot;

namespace Flamme.testing;

public partial class Bullet : Area2D
{
  [Export] public Vector2 Direction = Const.FacingNormVecDict[Const.Facing.Down];

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;

  private PlayerStats _playerStats;
  private bool _fired = false;
  private bool _destructing = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public void Fire(PlayerStats playerStats)
  {
    BodyEntered += OnBulletEntered;

    _playerStats = playerStats;
    GetTree().CreateTimer(_playerStats.Range).Timeout += InitBulletDestruction;
    _fired = true;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!_fired)
      return;
    
    Position += Direction * _playerStats.ShotSpeed;
  }

  private void OnBulletEntered(Node2D body)
  {
    if (body is Door door)
    {
      door.Open();
    }

    if (body is IPlayerDamageable enemy)
    {
      enemy.Damage(_playerStats.Damage, 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    InitBulletDestruction();
  }

  private void InitBulletDestruction()
  {
    if (_destructing)
      return;
    
    _destructing = true;
    
    var tween = GetTree().CreateTween();
    tween.TweenProperty(Sprite, Node2D.PropertyName.Scale.ToString(), Vector2.Zero, 0.1f);
    tween.TweenCallback(Callable.From(Sprite.QueueFree));
  }
}