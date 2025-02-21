using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

// ReSharper disable InvertIf

namespace Flamme.testing;

public partial class SimpleCharacter : Node2D
{
  [ExportGroup("Character")]
	[Export] public float SpeedMultiplier = 25.0f;
	[Export] public float FrictionMultiplier = .1f;
  [Export] public float MaxSpeed = 150.0f;
  [Export] public float ProjectileSpawnFromPlayer = 30.0f; 
  
  [ExportGroup("Staff")]
  [Export] public float StaffDistanceFromPlayer = 15.0f;
  [Export] public float MaxSpeedStaff = 500.0f;
  [Export] public float StaffFrictionMultiplier = .1f;
  [ExportSubgroup("Snapping")]
  [Export] public float SnapForceStaff = 500.0f;
  [Export] public float StaffSnapFrictionMultiplier = .4f;
  [Export] public float AngularFrictionMultiplierStaff = .1f;
  [ExportSubgroup("Trailing")]
  [Export] public float StaffCharDistStartTrail = 150.0f;
  [Export] public float StaffCharDistStopTrail = 50.0f;
  [Export] public float TrailForceStaff = 40.0f;
  [Export] public float StaffTrailFrictionMultiplier = .04f;

  [ExportGroup("Meta")] 
  [Export] public CharacterBody2D Body;
  [Export] public Area2D BodyArea;
  [Export] public Camera2D Camera;
  [Export] public RigidBody2D Staff;
  [Export] public Area2D StaffArea;
  [Export] public Sprite2D CharSprite;
  [Export] public AnimationPlayer StaffIdleAnimationPlayer;
  [Export] public PackedScene Bullet;
  [Export] public Timer BulletCooldownTimer;
  [Export] public AtlasTexture TextureCharUp;
  [Export] public AtlasTexture TextureCharDown;
  [Export] public AtlasTexture TextureCharLeft;
  [Export] public AtlasTexture TextureCharRight;
  
  private readonly Dictionary<Const.Facing, float> _facingStaffRotationDict = new Dictionary<Const.Facing, float>()
  {
    { Const.Facing.Up, Mathf.DegToRad(270) },
    { Const.Facing.Down, Mathf.DegToRad(0) },
    { Const.Facing.Left, Mathf.DegToRad(-45 + 5) },
    { Const.Facing.Right, Mathf.DegToRad(-45 - 5) },
  };

  private Dictionary<Const.Facing, AtlasTexture> _facingCharTextureDict;

  private Const.InputMap.Action _currentActionsBmap = Const.InputMap.Action.None;

  private Dictionary<Const.InputMap.Action, Action> _actionInputActionDict;

  private Const.Facing _currentFacing = Const.Facing.Down;

  private bool _isTrailing = false;
  
  private bool _isBulletOnCooldown = false;

  // Called when the node enters the scene tree for the first time.
	public override void _Ready()
  {
    // Needs to be here
    _facingCharTextureDict = new Dictionary<Const.Facing, AtlasTexture>()
    {
      { Const.Facing.Up, TextureCharUp },
      { Const.Facing.Down, TextureCharDown },
      { Const.Facing.Left, TextureCharLeft },
      { Const.Facing.Right, TextureCharRight }
    };
    
    _actionInputActionDict = new Dictionary<Const.InputMap.Action, Action>
    {
      { Const.InputMap.Action.MoveUp, () => Body.Velocity += Vector2.Up * SpeedMultiplier },
      { Const.InputMap.Action.MoveDown, () => Body.Velocity += Vector2.Down * SpeedMultiplier },
      { Const.InputMap.Action.MoveLeft, () => Body.Velocity += Vector2.Left * SpeedMultiplier },
      { Const.InputMap.Action.MoveRight,  () => Body.Velocity += Vector2.Right * SpeedMultiplier },
    };
    
		ExportMetaNonNull.Check(this);
    
    BulletCooldownTimer.Timeout += OnBulletCooldownTimer;
    BodyArea.AreaEntered += AreaEntered;
    
    StaffIdleAnimationPlayer.Play("Staff Idle");
	}

  private void AreaEntered(Area2D area)
  {
    if (area is Room newRoom)
    {
      var roomRect = newRoom.CollisionShape.Shape.GetRect();
      Camera.LimitLeft = (int)area.GlobalPosition.X;
      Camera.LimitTop = (int)area.GlobalPosition.Y;
      Camera.LimitRight = Camera.LimitLeft + (int)roomRect.Size.X;
      Camera.LimitBottom = Camera.LimitTop + (int)roomRect.Size.Y;
    }
  }

  public override void _UnhandledKeyInput(InputEvent @event)
  {
    // Iterate over each possible action
    foreach (var pair in Const.InputMap.ActionInputDict)
    {
      if (@event.IsActionPressed(pair.Value))
      {
        _currentActionsBmap |= pair.Key;
        GetViewport().SetInputAsHandled();
        break;
      }

      if (@event.IsActionReleased(pair.Value))
      {
        _currentActionsBmap &= ~pair.Key;
        GetViewport().SetInputAsHandled();
        break;
      }
    }
    
    // Efficiency increase possible here
    var newFacing = _currentFacing;
    if ((_currentActionsBmap & Const.InputMap.Action.ShootDown) > 0)
    {
      newFacing = Const.Facing.Down;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.ShootUp) > 0)
    {
      newFacing = Const.Facing.Up;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.ShootLeft) > 0)
    {
      newFacing = Const.Facing.Left;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.ShootRight) > 0)
    {
      newFacing = Const.Facing.Right;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.MoveDown) > 0)
    {
      newFacing = Const.Facing.Down;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.MoveUp) > 0)
    {
      newFacing = Const.Facing.Up;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.MoveLeft) > 0)
    {
      newFacing = Const.Facing.Left;
    }
    else if ((_currentActionsBmap & Const.InputMap.Action.MoveRight) > 0)
    {
      newFacing = Const.Facing.Right;
    }

    if (newFacing != _currentFacing)
    {
      _currentFacing = newFacing;
      CharSprite.Texture = _facingCharTextureDict[_currentFacing];
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    // Get all actions for every active flag
    var actions = _actionInputActionDict.Where(kv => _currentActionsBmap.HasFlag(kv.Key)).Select(kv => kv.Value);

    // Run every action from the bitmap
    foreach (var action in actions)
    {
      action();
    }
    
    Body.Velocity = Body.Velocity.LimitLength(MaxSpeed);
    Body.Velocity = Body.Velocity.Lerp(Vector2.Zero, FrictionMultiplier);
    Body.MoveAndSlide();
    
    // If actively shooting, the staff should snap to the right direction
    var isShooting = (_currentActionsBmap & (Const.InputMap.Action.ShootDown 
                                            | Const.InputMap.Action.ShootUp 
                                            | Const.InputMap.Action.ShootLeft 
                                            | Const.InputMap.Action.ShootRight)) > 0;
    
    // If too far from character, staff should trail behind, can't snap and trail at the same time though
    if (!_isTrailing && !isShooting && Body.GlobalPosition.DistanceTo(Staff.GlobalPosition) > StaffCharDistStartTrail)
    {
      _isTrailing = true;
    }
    else if(_isTrailing && Body.GlobalPosition.DistanceTo(Staff.GlobalPosition) < StaffCharDistStopTrail)
    {
      _isTrailing = false;
    }
    
    // Projectile stuff stuff
    if (isShooting && !_isBulletOnCooldown)
    {
      var bullet = Bullet.Instantiate<Bullet>();
      AddChild(bullet);
      bullet.Owner = GetTree().Root;
      bullet.Position = Body.Position + (Const.FacingNormVecDict[_currentFacing] * ProjectileSpawnFromPlayer);
      bullet.SetDirection(_currentFacing);
      _isBulletOnCooldown = true;
      BulletCooldownTimer.Start();
    }
      
      // Staff and snapping stuff
    if (isShooting)
    {
      var targetVec = Body.GlobalPosition + (Const.FacingNormVecDict[_currentFacing] * StaffDistanceFromPlayer) - Staff.GlobalTransform.Origin;
      var direction = targetVec.Normalized();
      var distance = targetVec.Length();
      Staff.ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 200) * SnapForceStaff);
      Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffSnapFrictionMultiplier); // Friction
    }
    else if(_isTrailing)
    {
      // Otherwise, just trail behind the player if they're too far away
      var targetVec = Body.GlobalPosition - Staff.GlobalTransform.Origin;
      var direction = targetVec.Normalized();
      var distance = targetVec.Length();
      Staff.ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 10) * TrailForceStaff);
      Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffTrailFrictionMultiplier); // Friction
    }
    else
    {
      Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffFrictionMultiplier);
    }
    
    // Max speed
    Staff.LinearVelocity = Staff.LinearVelocity.LimitLength(MaxSpeedStaff * 10);
    
    var overlapsPlayer = StaffArea.GetOverlappingBodies().Contains(Body);
    var overlapMinusPlayer = StaffArea.GetOverlappingBodies().Count - (overlapsPlayer ? 1 : 0);
    
    if (isShooting && overlapMinusPlayer == 0 && Mathf.Abs(_facingStaffRotationDict[_currentFacing] - Staff.Rotation) > 0.01)
    {
      var targetRotation = _facingStaffRotationDict[_currentFacing];
      var angleDiff = Mathf.PosMod(_facingStaffRotationDict[_currentFacing] - Staff.Rotation, Mathf.Tau);

      if (angleDiff > Mathf.Pi)
      {
        angleDiff -= Mathf.Tau;
      }
      else
      {
        angleDiff += Mathf.Tau;
      }
      Staff.AngularVelocity = angleDiff * 5;
    }
    
    // Angular friction
    Staff.AngularVelocity = Mathf.Lerp(Staff.AngularVelocity, 0, AngularFrictionMultiplierStaff);
  }

  private void OnBulletCooldownTimer()
  {
    _isBulletOnCooldown = false;
  }
}