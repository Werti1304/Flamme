using Godot;
using System;
using Flamme;

public partial class FireflyNeutral : Enemy
{
  [Export] public float Speed = 5.0f;
  [Export] public double NewDirectionTimeMax = 3.0f;
  
  [ExportGroup("Meta")]
  [Export] public AnimatedSprite2D Sprite;
  private double _newDirectionTimer;
  
  protected Vector2 Direction = Vector2.Zero;

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;

    if (_newDirectionTimer >= NewDirectionTimeMax)
    {
      _newDirectionTimer = 0.0f;
      Direction = GetNewDirection();
    }
    else if(Main.Instance.Rnd.Randf() < 0.2f)
    {
      Direction += new Vector2((Main.Instance.Rnd.Randf() - 0.5f) / 2.0f, (Main.Instance.Rnd.Randf() - 0.5f) / 2.0f);
      Direction = Direction.Normalized();
      _newDirectionTimer += delta;
    }
    else
    {
      _newDirectionTimer += delta;
    }
    
    var direction = (Target.GlobalPosition - GlobalPosition).Normalized();
    if (direction.X < 0 && Sprite.FlipH)
    {
      Sprite.FlipH = false;
    }
    else if (direction.X > 0 && !Sprite.FlipH)
    {
      Sprite.FlipH = true;
    }
    Velocity = Velocity.Lerp(Direction * Speed, 0.3f);
        
    MoveAndSlide();
  }

  protected virtual Vector2 GetNewDirection()
  {
    return new Vector2((Main.Instance.Rnd.Randf() - 0.5f) * 2.0f, (Main.Instance.Rnd.Randf() - 0.5f) * 2.0f);
  }
}
