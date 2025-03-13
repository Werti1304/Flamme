using Godot;
using System;

public partial class FireflyRagingLight : FireflyNeutral
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
