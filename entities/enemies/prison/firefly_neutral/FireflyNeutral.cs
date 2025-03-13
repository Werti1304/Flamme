using Godot;
using System;
using Flamme;

public partial class FireflyNeutral : Enemy
{
  [Export] public float Speed = 5.0f;
  
  [ExportGroup("Meta")]
  [Export] public AnimatedSprite2D Sprite;

  private double _newDirectionTimeMax = 3.0f;
  private double _newDirectionTimer;
  
  private Vector2 _direction = Vector2.Zero;

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;

    if (_newDirectionTimer >= _newDirectionTimeMax)
    {
      _newDirectionTimer = 0.0f;
      _direction = new Vector2((Main.Instance.Rnd.Randf() - 0.5f) * 2.0f, (Main.Instance.Rnd.Randf() - 0.5f) * 2.0f);
    }
    else if(Main.Instance.Rnd.Randf() < 0.2f)
    {
      _direction += new Vector2((Main.Instance.Rnd.Randf() - 0.5f) / 2.0f, (Main.Instance.Rnd.Randf() - 0.5f) / 2.0f);
      _direction = _direction.Normalized();
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
    Velocity = Velocity.Lerp(_direction * Speed, 0.3f);
        
    MoveAndSlide();
  }
}
