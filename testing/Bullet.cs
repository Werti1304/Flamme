using Godot;
using System;

public partial class Bullet : Area2D
{
  [Export] public int Speed = 750;
  [Export] private Vector2 _direction = Const.FacingNormVecDict[Const.Facing.Down];

  public override void _Ready()
  {
    BodyEntered += OnBulletEntered;
  }

  public override void _PhysicsProcess(double delta)
  {
    Position += _direction * Speed * (float)delta;
  }

  private void OnBulletEntered(Node2D body)
  {
    QueueFree();
  }

  public void SetDirection(Const.Facing facing)
  {
    _direction = Const.FacingNormVecDict[facing];
  }
}
