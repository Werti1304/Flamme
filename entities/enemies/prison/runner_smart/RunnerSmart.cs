using Flamme.common.helpers;
using Godot;

namespace Flamme.entities.enemies.prison.runner_smart;

public partial class RunnerSmart : Enemy
{
  [Export] public float Speed = 120.0f;
  
  [ExportGroup("Meta")]
  [Export] public NavigationAgent2D NavigationAgent;
  [Export] public Sprite2D Sprite;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    NavigationAgent.TargetPosition = Target.GlobalPosition;
    
    var nextPos = NavigationAgent.GetNextPathPosition();
    var direction = (nextPos - GlobalPosition).Normalized();
    Velocity = Velocity.Lerp(direction * Speed, 0.02f);
    
    if (direction.X < 0 && Sprite.FlipH)
    {
      Sprite.FlipH = false;
    }
    else if (direction.X > 0 && !Sprite.FlipH)
    {
      Sprite.FlipH = true;
    }
    
    MoveAndSlide();
  }

  public override void OnDeath()
  {
    QueueFree();
  }
}