using Godot;
using System;

public partial class FireflyHoming : FireflyNeutral
{
  [Export] public float Range = 192.0f;
  [Export] public float ShootTimerSec = 3.0f;
  
  [ExportGroup("Meta")]
  [Export] public Shooter Shooter;
  
  protected override Vector2 GetNewDirection()
  {
    if (GlobalPosition.DistanceTo(Target.GlobalPosition) > Range)
    {
      return (Target.GlobalPosition - GlobalPosition).Normalized();
    }
    return base.GetNewDirection();
  }

  private double _shootTimer;
  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    base._PhysicsProcess(delta);
    
    _shootTimer += delta;
    if (_shootTimer > ShootTimerSec)
    {
      _shootTimer = 0.0f;
      Shooter.Shoot(this, Target);
    }
  }
}
