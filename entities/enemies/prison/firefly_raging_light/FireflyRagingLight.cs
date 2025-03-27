using Godot;

namespace Flamme.entities.enemies.prison.firefly_raging_light;

public partial class FireflyRagingLight : firefly_neutral.FireflyNeutral
{
  protected override void OnSetActive()
  {
    Direction = GetNewDirection();
  }

  protected override Vector2 GetNewDirection()
  {
    return (Target.GlobalPosition - GlobalPosition).Normalized();
  }
}