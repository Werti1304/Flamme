using Flamme.entities.common;
using Flamme.entities.enemies.components.melee_area;
using Godot;
using GodotPlugins.Game;
using System;
using Main = Flamme.Main;

public partial class Slider : Enemy
{
  [Export] public float Speed = 300.0f;
  [Export] public float AccelerationWeight = 0.1f;
  [Export] public float KnockbackMultiplier = 2.0f;
  
  [ExportGroup("Meta")]
  [Export] public HealthBar HealthBar;
  [Export] public Sprite2D Body;
  [Export] public Sprite2D Eye;
  [Export] public Node2D EyeCenter;
  [Export] public Timer ChangeDirectionTimer;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public MeleeArea MeleeArea;

  private static readonly Vector2I EyeSize = new Vector2I(10, 5);
  
  private Vector2 _direction;
  private double _directionTimerDefault;
  
  public override void _Ready()
  {
    base._Ready();
    
    _directionTimerDefault = ChangeDirectionTimer.WaitTime;
    
    HealthChanged += HealthBar.OnHealthChanged;
    
    HealthBar.OnHealthChanged(this);
    
    ChangeDirectionTimer.Timeout += ChangeDirectionTimerOnTimeout;
    
    MeleeArea.BodyEntered += MeleeAreaOnBodyEntered;
  }

  private void MeleeAreaOnBodyEntered(Node2D body)
  {
    if (body is PlayableCharacter playableCharacter)
    {
      playableCharacter.Velocity += GlobalPosition.DirectionTo(body.GlobalPosition) * Mathf.Max(200.0f, Velocity.Length() * KnockbackMultiplier);
    }
  }

  protected override void OnSetActive()
  {
    ChangeDirectionTimer.Start();
  }

  protected override void OnSetPassive()
  {
    ChangeDirectionTimer.Stop();
  }

  private void ChangeDirectionTimerOnTimeout()
  {
    if (Target == null)
      return;
    
    var directionTo = GlobalPosition.DirectionTo(Target.GlobalPosition);
    var directionToAbs = directionTo.Abs();
    
    // Target is around diagonal to us, so theres a chance we go diagonal to him
    if (directionToAbs.X - directionToAbs.Y < 0.5f && Main.Instance.Rnd.Randf() < 0.3f)
    {
      _direction = directionTo;
    }
    else if (directionToAbs.X > directionToAbs.Y)
    {
      _direction = new Vector2(directionTo.X, 0);
    }
    else
    {
      _direction = new Vector2(0, directionTo.Y);
    }
    ChangeDirectionTimer.WaitTime = _directionTimerDefault * 0.5f + Main.Instance.Rnd.Randf() * 0.8f;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    var lookingDirection = (Target.GlobalPosition - EyeCenter.GlobalPosition).Normalized();
    var pupilPosition = (Vector2I)(EyeCenter.Position + new Vector2I((int)(lookingDirection.X * EyeSize.X), (int)(lookingDirection.Y * EyeSize.Y)));
    Eye.Position = pupilPosition;
    
    Velocity = Velocity.Lerp(_direction * Speed, AccelerationWeight);

    MoveAndSlide();
  }
}
