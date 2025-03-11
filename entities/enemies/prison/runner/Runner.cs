
using Godot;

// TODO Add possible loot to all enemies
public partial class Runner : Enemy
{
  [Export] public float Speed = 20.0f;
  
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;

  private bool _stunned = false;
  private double _stunnedTime = 0;
  private double _stunnedTimeCounter = 0;
  
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

    if (_stunned)
    {
      // If stunned, runner got knocked back and now only has friction
      Velocity = Velocity.Lerp(Vector2.Zero, .1f);
      _stunnedTimeCounter += delta;

      if (_stunnedTimeCounter > _stunnedTime)
      {
        _stunned = false;
      }
    }
    else
    {
      var direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
      if (direction.X < 0 && !Sprite.FlipH)
      {
        Sprite.FlipH = true;
      }
      else if (direction.X > 0 && Sprite.FlipH)
      {
        Sprite.FlipH = false;
      }
      Velocity = direction * Speed;
    }
        
    MoveAndSlide();
  }

  public override void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    base.Hit(attackDamage, knockBackStrength, attackDirection);

    Velocity = knockBackStrength * attackDirection;
    _stunned = true;
    _stunnedTime = knockBackStrength / 100.0f;
  }
}
