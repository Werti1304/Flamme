using Godot;
using System;
using Flamme.world;

public partial class MageWeak : Enemy
{
  [Export] public float Speed = 10.0f;
  [Export] public float ShootTimerSec = 3.0f;

  [Export] public float Range = 96.0f;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public RayCast2D RayCast;
  [Export] public NavigationAgent2D NavigationAgent;
  [Export] public Shooter Shooter;
  
  private double _shootTimer;

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    RayCast.TargetPosition = RayCast.ToLocal(Target.GlobalPosition);
    if (Target.GlobalPosition.DistanceTo(GlobalPosition) < Range 
      && RayCast.IsColliding() && RayCast.GetCollider() is PlayableCharacter)
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

  private int _shootingCounter = 0;
  private void ShootingTimerOnTimeout()
  {
    Shooter.ProjectileCount = _shootingCounter + 1;
    Shooter.Shoot(this, Target);
    if (_shootingCounter == 1)
      _shootingCounter = 0;
    else
      _shootingCounter++;
  }
}
