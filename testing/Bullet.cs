using Flamme.entities;
using Godot;

namespace Flamme.testing;

public partial class Bullet : Area2D
{
  [Export] public int Speed = 200;
  [Export] public float DespawnTime = 5;
  [Export] public Vector2 Direction = Const.FacingNormVecDict[Const.Facing.Down];

  public float Damage = 3;

  public override void _Ready()
  {
    BodyEntered += OnBulletEntered;

    GetTree().CreateTimer(DespawnTime).Timeout += QueueFree;
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
      enemy.Damage(Damage, 100, (body.GlobalPosition - GlobalPosition).Normalized());
    }
    QueueFree();
  }
}