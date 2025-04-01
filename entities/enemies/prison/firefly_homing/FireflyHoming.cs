using Godot;

namespace Flamme.entities.enemies.prison.firefly_homing;

public partial class FireflyHoming : firefly_neutral.FireflyNeutral
{
  [Export] public float Range = 192.0f;
  [Export] public float ShootTimerSec = 3.0f;
  
  [ExportGroup("Meta")]
  [Export] public components.shooter.Shooter Shooter;
  
  protected override Vector2 GetNewDirection()
  {
    if (GlobalPosition.DistanceTo(Target.GlobalPosition) > Range)
    {
      return (Target.GlobalPosition - GlobalPosition).Normalized();
    }
    return base.GetNewDirection();
  }

  private double _shootTimer;
  protected override void PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    _shootTimer += delta;
    if (_shootTimer > ShootTimerSec)
    {
      _shootTimer = 0.0f;
      Shooter.Shoot(this, Target);
    }
    
    Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
  }
}