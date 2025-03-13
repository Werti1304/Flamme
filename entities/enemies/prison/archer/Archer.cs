using Godot;
using System;
using Flamme.world;

public partial class Archer : Enemy
{
  [Export] public float Speed = 10.0f;
  [Export] public float ShootTimerSec = 3.0f;

  [Export] public float ShootStartDistance = 24.0f;
  [Export] public PackedScene ProjectileScene;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public RayCast2D RayCast;
  [Export] public NavigationAgent2D NavigationAgent;

  private double _shootTimer;

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    RayCast.TargetPosition = RayCast.ToLocal(Target.GlobalPosition);
    if (RayCast.IsColliding() && RayCast.GetCollider() is PlayableCharacter)
    {
      Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
      
      var direction = (ToGlobal(RayCast.TargetPosition) - GlobalPosition).Normalized();
      if (direction.X < 0 && Sprite.FlipH)
      {
        Sprite.FlipH = false;
      }
      else if (direction.X > 0 && !Sprite.FlipH)
      {
        Sprite.FlipH = true;
      }
      
      // Shoot here
      if (_shootTimer >= ShootTimerSec)
      {
        ShootingTimerOnTimeout();
        _shootTimer = 0.0f;
      }
      else
      {
        _shootTimer += delta;
      }
    }
    else
    {
      NavigationAgent.TargetPosition = Target.GlobalPosition;
      var nextPos = NavigationAgent.GetNextPathPosition();
      
      var direction = (nextPos - GlobalPosition).Normalized();
      Velocity = Velocity.Lerp(direction * Speed, 0.05f);
      
      if (direction.X < 0 && Sprite.FlipH)
      {
        Sprite.FlipH = false;
      }
      else if (direction.X > 0 && !Sprite.FlipH)
      {
        Sprite.FlipH = true;
      }
    }
    
    MoveAndSlide();
  }

  private void ShootingTimerOnTimeout()
  {
    var projectile = ProjectileScene.Instantiate<EnemyProjectile>();
    var direction = (Target.GlobalPosition - GlobalPosition).Normalized();
    projectile.GlobalPosition = GlobalPosition + (direction * ShootStartDistance);
    projectile.Direction = direction;
    var targetRotation = projectile.Direction.Angle();
    projectile.Rotation = targetRotation;
    GetTree().Root.AddChild(projectile);
    projectile.Fire(this, LevelManager.Instance.CurrentRoom, 1000.0f);
  }
}
