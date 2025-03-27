namespace Flamme.projectiles.enemy.grand_mage;

public partial class BigHoming : EnemyProjectile
{
  public override void _PhysicsProcess(double delta)
  {
    if (!IsInstanceValid(Target))
    {
      return;
    }
    var exactDirection = Target.GlobalPosition - GlobalPosition;
    Direction = Direction.Lerp(exactDirection, 0.001f).Normalized();
    base._PhysicsProcess(delta);
  }
}