using Flamme.common.helpers;
using Godot;

namespace Flamme.entities.enemies.prison.runner_angry;

public partial class RunnerAngry : Enemy
{
  [Export] public float Speed = 180.0f;
  [Export] public float TriggerDistance = 96.0f;
  
  [ExportGroup("Meta")]
  [Export] public NavigationAgent2D NavigationAgent;
  [Export] public Sprite2D PassiveSprite;
  [Export] public Sprite2D ActiveSprite;

  private bool _isTriggered = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    ActiveSprite.Hide();
  }

  private void SetTriggered(bool triggered)
  {
    if (triggered == _isTriggered)
      return;
    
    if (triggered)
    {
      PassiveSprite.Hide();
      ActiveSprite.Show();
    }
    else
    {
      PassiveSprite.Show();
      ActiveSprite.Hide();
    }
    _isTriggered = triggered;
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if(!IsActive)
      return;

    // If player < TriggerDistance Away, set active.
    // Also gets set to active when attacked
    if (!_isTriggered)
    {
      if (Target.GlobalPosition.DistanceTo(GlobalPosition) < TriggerDistance)
      {
        SetTriggered(true);
      }
      else
      {
        return;
      }
    }
    
    // Enemy is triggered, e.g. active
    NavigationAgent.TargetPosition = Target.GlobalPosition;
    
    var nextPos = NavigationAgent.GetNextPathPosition();
    var direction = (nextPos - GlobalPosition).Normalized();
    Velocity = Velocity.Lerp(direction * Speed, 0.1f);
    
    if (direction.X < 0 && ActiveSprite.FlipH)
    {
      ActiveSprite.FlipH = false;
    }
    else if (direction.X > 0 && !ActiveSprite.FlipH)
    {
      ActiveSprite.FlipH = true;
    }
    
    MoveAndSlide();
  }

  protected override void OnSetPassive()
  {
    SetTriggered(false);
  }

  protected override void OnHit()
  {
    SetTriggered(true);
  }
}