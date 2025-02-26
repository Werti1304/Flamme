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

  private PlayableCharacter _owner;
  private bool _staffOverlappingWithPlayer = false;
  private bool _snapped = false;
  private bool _trailing = false;

  // TODO 2 make pulsate when no owner yet
  // TODO 2 add shadow below and hovering of staff when neither trailing nor shooting (but has owner)
  // TODO 2 maybe put snapping movement into integrated force as linear so that weird movement still snaps
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    PickupArea.BodyEntered += PickupAreaOnBodyEntered;
    
    Area.BodyEntered += AreaOnBodyEntered;
    Area.BodyExited += AreaOnBodyExited;
  }
  
  public override void _PhysicsProcess(double delta)
  {
    if (_owner == null)
      return;
    
    CheckSnap();
    
    // --- Shooting / Snapping ---
    
    if (_owner.IsShooting)
    {
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
    
    // if (!_isTrailing && !isShooting && GlobalPosition.DistanceTo(Staff.GlobalPosition) > StaffCharDistStartTrail)
    // {
    //   _isTrailing = true;
    // }
    // else if(_isTrailing && GlobalPosition.DistanceTo(Staff.GlobalPosition) < StaffCharDistStopTrail)
    // {
    //   _isTrailing = false;
    // }
    //
    // else if(_isTrailing)
    // {
    //   // Otherwise, just trail behind the player if they're too far away
    //   var targetVec = GlobalPosition - Staff.GlobalTransform.Origin;
    //   var direction = targetVec.Normalized();
    //   var distance = targetVec.Length();
    //   Staff.ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 10) * TrailForceStaff);
    //   Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffTrailFrictionMultiplier); // Friction
    // }
    // else
    // {
    //   Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffFrictionMultiplier);
    // }
    //
    // // Max speed
    // Staff.LinearVelocity = Staff.LinearVelocity.LimitLength(MaxSpeedStaff * 10);
    //
    // var overlapsPlayer = Staff.Area.GetOverlappingBodies().Contains(this);
    // var overlapMinusPlayer = Staff.Area.GetOverlappingBodies().Count - (overlapsPlayer ? 1 : 0);
    //
    // if (isShooting && overlapMinusPlayer == 0 && Mathf.Abs(_facingStaffRotationDict[CurrentFacing] - Staff.Rotation) > 0.01)
    // {
    //   var targetRotation = _facingStaffRotationDict[CurrentFacing];
    //   var angleDiff = Mathf.PosMod(_facingStaffRotationDict[CurrentFacing] - Staff.Rotation, Mathf.Tau);
    //
    //   if (angleDiff > Mathf.Pi)
    //   {
    //     angleDiff -= Mathf.Tau;
    //   }
    //   else
    //   {
    //     angleDiff += Mathf.Tau;
    //   }
    //   Staff.AngularVelocity = angleDiff * 10;
    // }
    //
    // // Angular friction
    // Staff.AngularVelocity = Mathf.Lerp(Staff.AngularVelocity, 0, AngularFrictionMultiplierStaff);
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
    PickupArea.SetDeferred("Monitoring", false);
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