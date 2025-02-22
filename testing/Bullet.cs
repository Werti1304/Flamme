using Godot;
using System;

public partial class Bullet : Area2D
{
  [Export] public int Speed = 200;
  [Export] public Vector2 Direction = Const.FacingNormVecDict[Const.Facing.Down];

  public override void _Ready()
  {
    BodyEntered += OnBulletEntered;
  }

  public override void _PhysicsProcess(double delta)
  {
    Position += Direction * Speed * (float)delta;
  }

  private void OnBulletEntered(Node2D body)
  {
    if (body is Door door)
    {
      door.Open();
    }

    if (body is IPlayerDamageable enemy)
    {
      enemy.Damage(3, 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    QueueFree();
  }
}
