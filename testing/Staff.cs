using Flamme.testing;
using Godot;
using System;

public partial class Staff : RigidBody2D
{
  [ExportGroup("Meta")]
  [Export] public Area2D Area;
  [Export] public AnimationPlayer IdleAnimationPlayer;

  public bool ShouldReset = false;
  public Vector2 ResetPos;

  public override void _IntegrateForces(PhysicsDirectBodyState2D state)
  {
    if (!ShouldReset)
      return;
    
    ShouldReset = false;
    state.Transform = new Transform2D(state.Transform.Rotation, ResetPos);
  }

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }
}
