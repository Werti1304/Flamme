namespace Flamme.projectiles.enemy.firefly_homer;

public partial class FireflyHomer : EnemyProjectile
{
  public override void _Ready()
  {
    // TODO 1 Do in editor just buggy rn
    DissipateTime = 0.3f;
  }
  
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