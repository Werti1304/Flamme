
using Godot;

// TODO Add possible loot to all enemies
namespace Flamme.entities.enemies.prison.runner;

public partial class Runner : Enemy
{
  [Export] public float Speed = 100.0f;
  
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  
  public override void OnDeath()
  {
    QueueFree();
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);
    
    // TODO 1 Relevant for pretty much all _PhysicsProcess -> enable PhysicProcess only when needed in addition to these ifs
    // Not yet tho cuz else Im gonna miss something and get funny bugs
    if (Target == null)
      return;
    
    var direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
    if (direction.X < 0 && Sprite.FlipH)
    {
      Sprite.FlipH = false;
    }
    else if (direction.X > 0 && !Sprite.FlipH)
    {
      Sprite.FlipH = true;
    }
    Velocity = Velocity.Lerp(direction * Speed, 0.1f);
        
    MoveAndSlide();
  }
}