using Flamme.entities.player;
using Flamme.testing;
using Flamme.world;
using Flamme.world.generation;
using Godot;
using System;

namespace Flamme.entities.staff;

public partial class Staff : RigidBody2D
{
  [Export] public float BaseFriction = .1f;
  [ExportCategory("Staff Player Interaction")]
  [Export] public float DistanceFromPlayer = 16;
  [ExportGroup("Snapping")] 
  [Export] public float SnapSpeed = 700.0f;
  [Export] public float SnapDamp = 0.3f;
  [ExportGroup("Trailing")] 
  [Export] public float TrailingForce = 0f; // Deprecated; Set according to player speed stat now 
  [Export] public float TrailingFrictionWeight = .3f;
  [Export] public float DistanceToStartTrailing = 60.0f;
  [Export] public float DistanceToStopTrailing = 40.0f;
  [ExportGroup("Shooting")] 
  [Export] public float ShootDistanceFromStaff = 10.0f;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D StaffSprite;
  [Export] public Sprite2D StaffCore;
  [Export] public Area2D PickupArea;
  [Export] public Area2D Area;
  [Export] public PinJoint2D PinJoint;
  [Export] public CollisionShape2D CollisionShape;

  private PlayableCharacter _owner;
  private bool _staffOverlappingWithPlayer = false;
  private bool _snapped = false;
  private bool _trailing = false;
  private bool _collisionDisabled = false;
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
  }

  private double _shootTimer = 0.0f;
  private double _shootTimerMax = 100.0f;
  public override void _PhysicsProcess(double delta)
  {
    if (_owner == null || !IsInstanceValid(_owner))
      return;
    
    CheckSnap();
    
    // --- Shooting / Snapping ---
    
    if (_owner.IsShooting)
    {
      if (_shootTimer >= _shootTimerMax)
      {
        ShootingTimerOnTimeout();
        _shootTimer = 0.0f;
      }
      else
      {
        _shootTimer += delta;
      }
      
      // When shooting, remove all collision from staff
      if (!_collisionDisabled)
      {
        _collisionDisabled = true;
        CollisionShape.SetDeferred("disabled", true);
      }

      // Move Staff towards player
      var targetVec = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer) - GlobalPosition; // ;
      var direction = targetVec.Normalized();
      var distance = targetVec.Length();
      // ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 200) * SnapForce);
      return;
    }
    
    // Not snapping anymore, enable collision again for staff
    if (_collisionDisabled && Area.GetOverlappingBodies().Count == 0)
    {
      _collisionDisabled = false;
      CollisionShape.SetDeferred("disabled", false);
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
      var targetPos = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer);
      var rotationDirection = _owner.GlobalPosition.DirectionTo(targetPos);
      var targetRotation = rotationDirection.Angle();
      Rotation = targetRotation;

      var targetVec = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer) - GlobalPosition; // ;
      var direction = targetVec.Normalized();
      LinearVelocity = LinearVelocity.Lerp(direction * SnapSpeed, SnapDamp);
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
    _shootTimerMax = _owner.Stats.FireRate / 10.0f;
  }

  private void ShootingTimerOnTimeout()
  {
    if (!_owner.IsShooting)
    {
      return;
    }
    _tween?.Kill();
    _tween = GetTree().CreateTween();

    _tween.TweenProperty(StaffCore, "modulate:a", 1, 0.2f).SetTrans(Tween.TransitionType.Sine)
      .Finished += Shoot;
    _tween.TweenProperty(StaffCore, "modulate:a", 0, _shootTimerMax / 2.0f)
      .SetTrans(Tween.TransitionType.Sine);
  }

  [Export] private PackedScene _projectile;
  
  private void Shoot()
  {
    // TODO In the works
    var projectile = _projectile.Instantiate<Trailing>();
    projectile.GlobalPosition = GlobalPosition + (_owner.ShootingVector * ShootDistanceFromStaff);
    // bullet.Direction = (_owner.ShootingVector + (_owner.Velocity / _owner.Stats.Speed)).Normalized();
    // if (_owner.Velocity.Length() > 10)
    // {
    //   projectile.Direction = 0.8f * _owner.ShootingVector + 0.4f * _owner.Velocity.Normalized();
    // }
    // else
    // {
    //   projectile.Direction = _owner.ShootingVector;
    // }
    projectile.Direction = _owner.ShootingVector;
    var targetRotation = projectile.Direction.Angle();
    projectile.Rotation = targetRotation;
    GetTree().Root.AddChild(projectile);
    projectile.Fire(_owner.Stats);
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
    LevelManager.Instance.CurrentLevel.ActiveStaff = this;
    OwnerOnStatsChanged(_owner.Stats);
    PickupArea.SetDeferred("Monitoring", false);
  }

  public void ClearOwner()
  {
    _owner.StatsChanged -= OwnerOnStatsChanged;
    _owner = null;
    LevelManager.Instance.CurrentLevel.ActiveStaff = null;
    PickupArea.SetDeferred("Monitoring", true);
  }

  private void OwnerOnStatsChanged(PlayerStats stats)
  {
    TrailingForce = stats.Speed / 3.0f;
    UpdateFireRate();
  }

  private void CheckSnap()
  {
    if (_owner.IsShooting)
    {
      var targetVec = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer) - GlobalPosition; // ;
      var distance = targetVec.Length();
      
      if (_snapped && distance > 10.0f)
      {
        SetSnap(false);
      }
      else if (!_snapped && distance < 5.0f)
      {
        SetDeferred(Node2D.PropertyName.GlobalPosition, _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer));
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
    if (snapEnabled)
    {
      PinJoint.NodeB = _owner.GetPath();
      SetDeferred(RigidBody2D.PropertyName.LockRotation, true);
      _snapped = true;
    }
    else
    {
      PinJoint.NodeB = null;
      SetDeferred(RigidBody2D.PropertyName.LockRotation, false);
      _snapped = false;
    }
  }
}