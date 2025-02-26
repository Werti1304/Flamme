using Flamme.entities.player;
using Flamme.testing;
using Godot;

namespace Flamme.entities.staff;

public partial class Staff : RigidBody2D
{
  [Export] public float BaseFriction = .1f;
  [ExportCategory("Staff Player Interaction")]
  [Export] public float DistanceFromPlayer = 15;
  [ExportGroup("Snapping")] 
  [Export] public float SnapForce = 500.0f;
  [Export] public float SnapFrictionWeight = .3f;
  [ExportGroup("Trailing")] 
  [Export] public float TrailingForce = 30.0f;
  [Export] public float TrailingFrictionWeight = .3f;
  [Export] public float DistanceToStartTrailing = 60.0f;
  [Export] public float DistanceToStopTrailing = 40.0f;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D StaffSprite;
  [Export] public Sprite2D StaffCore;
  [Export] public Area2D PickupArea;
  [Export] public Area2D Area;
  [Export] public PinJoint2D PinJoint;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Timer ShootingTimer;

  private PlayableCharacter _owner;
  private bool _staffOverlappingWithPlayer = false;
  private bool _snapped = false;
  private bool _trailing = false;
  private Tween _tween;

  // TODO 2 make pulsate when no owner yet
  // TODO 2 add shadow below and hovering of staff when neither trailing nor shooting (but has owner)
  // TODO 2 maybe put snapping movement into integrated force as linear so that weird movement still snaps
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    StaffCore.Modulate = StaffCore.Modulate with { A = 0 };
    
    PickupArea.BodyEntered += PickupAreaOnBodyEntered;
    
    Area.BodyEntered += AreaOnBodyEntered;
    Area.BodyExited += AreaOnBodyExited;
    
    ShootingTimer.Timeout += ShootingTimerOnTimeout;
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if (_owner == null)
      return;
    
    CheckSnap();
    
    // --- Shooting / Snapping ---
    
    if (_owner.IsShooting)
    {
      if (ShootingTimer.IsStopped())
      {
        ShootingTimer.Start();
      }
      
      // When shooting, remove all collision from staff
      if (!CollisionShape.Disabled)
      {
        CollisionShape.SetDeferred("Disabled", true);
      }

      // Move Staff towards player
      var targetVec = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer) - GlobalPosition; // ;
      var direction = targetVec.Normalized();
      var distance = targetVec.Length();
      ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 200) * SnapForce);
      return;
    }
    
    // Not snapping anymore, enable collision again for staff
    if (CollisionShape.Disabled)
    {
      CollisionShape.SetDeferred("Disabled", false);
    }

    if (!ShootingTimer.IsStopped())
    {
      ShootingTimer.Stop();
    }
    
    // --- Trailing ---
    
    if (!_trailing && GlobalPosition.DistanceTo(_owner.GlobalPosition) > DistanceToStartTrailing)
    {
      _trailing = true;
    }
    else if (_trailing && GlobalPosition.DistanceTo(_owner.GlobalPosition) < DistanceToStopTrailing)
    {
      _trailing = false;
    }

    if (!_trailing)
      return;
    
    var targetVecTrailing = _owner.GlobalPosition - GlobalPosition;
    var directionTrailing = targetVecTrailing.Normalized();
    var distanceTrailing = targetVecTrailing.Length();
    ApplyCentralForce(directionTrailing * Mathf.Pow(Mathf.Clamp(distanceTrailing, 0, 10), 2) * TrailingForce);
  }

  public override void _IntegrateForces(PhysicsDirectBodyState2D state)
  {
    if (_owner == null || _snapped)
      return;

    // Friction
    if (_owner.IsShooting)
    {
      var targetPos = _owner.Position + (_owner.ShootingVector * DistanceFromPlayer);
      var direction = _owner.Position.DirectionTo(targetPos);
      var targetRotation = direction.Angle();
      Rotation = targetRotation;
      LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, SnapFrictionWeight);
    }
    else if (_trailing)
    {
      LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, TrailingFrictionWeight);
    }
    else
    {
      LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, BaseFriction);
    }
  }

  public void UpdateFireRate()
  {
    ShootingTimer.Stop();
    ShootingTimer.WaitTime = 60.0f / _owner.Stats.FireRate;
  }

  private void ShootingTimerOnTimeout()
  {
    _tween?.Kill();
    _tween = GetTree().CreateTween();

    _tween.TweenProperty(StaffCore, "modulate:a", 1, 0.2f).SetTrans(Tween.TransitionType.Sine);
    _tween.TweenProperty(StaffCore, "modulate:a", 0, ShootingTimer.WaitTime / 2.0f)
      .SetTrans(Tween.TransitionType.Sine);
  }

  private void AreaOnBodyEntered(Node2D body)
  {
    if (body is not PlayableCharacter)
      return;
    
    _staffOverlappingWithPlayer = true;
  }

  private void AreaOnBodyExited(Node2D body)
  {
    if (body is not PlayableCharacter)
      return;

    _staffOverlappingWithPlayer = false;
  }

  private void PickupAreaOnBodyEntered(Node2D body)
  {
    if (_owner != null || body is not PlayableCharacter playableCharacter)
      return;
    
    _owner = playableCharacter;
    _owner.StatsChanged += OwnerOnStatsChanged;
    OwnerOnStatsChanged(_owner.Stats);
    PickupArea.SetDeferred("Monitoring", false);
  }

  private void OwnerOnStatsChanged(PlayerStats stats)
  {
    UpdateFireRate();
  }

  private void CheckSnap()
  {
    if (_owner.IsShooting)
    {
      var targetVec = _owner.Position + (_owner.ShootingVector * DistanceFromPlayer) - Position; // ;
      var distance = targetVec.Length();
      
      if (_snapped && distance > 10.0f)
      {
        SetSnap(false);
      }
      else if (!_snapped && distance < 3.0f)
      {
        SetSnap(true);
      }
    }
    else if (_snapped)
    {
      SetSnap(false);
    }
  }

  private void SetSnap(bool snapEnabled)
  {
    GD.Print($"Staff Snap: {snapEnabled}");
    if (snapEnabled)
    {
      PinJoint.NodeB = _owner.GetPath();
      _snapped = true;
    }
    else
    {
      PinJoint.NodeB = null;
      _snapped = false;
    }
  }
}