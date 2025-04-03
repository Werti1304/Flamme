using Flamme.common.helpers;
using Flamme.entities.player;
using Flamme.projectiles.player;
using Flamme.world;
using Flamme.world.generation;
using Flamme.world.rooms;
using Godot;
using System;

namespace Flamme.entities.staff;

public partial class Staff : RigidBody2D
{
  [Export] public float BaseFriction = .1f;
  [ExportCategory("Staff Player Interaction")]
  [Export] public float DistanceFromPlayer = 12;
  [ExportGroup("Snapping")] 
  [Export] public float SnapSpeed = 700.0f;
  [Export] public float SnapDamp = 0.3f;
  [ExportGroup("Trailing")] 
  [Export] public float TrailingForce; // Deprecated; Set according to player speed stat now 
  [Export] public float TrailingFrictionWeight = .3f;
  [Export] public float DistanceToStartTrailing = 60.0f;
  [Export] public float DistanceToStopTrailing = 40.0f;
  [ExportGroup("Shooting")] 
  [Export] public float ShootDistanceFromStaff = 2.0f;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D StaffSprite;
  [Export] public Sprite2D StaffCore;
  [Export] public Area2D PickupArea;
  [Export] public Area2D Area;
  [Export] public PinJoint2D PinJoint;
  [Export] public CollisionShape2D CollisionShape;

  public bool Snapped { get; private set; }
  
  private player.PlayableCharacter _owner;
  private bool _staffOverlappingWithPlayer;
  private bool _trailing;
  private bool _collisionDisabled;
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

  private double _shootTimer;
  private double _shootTimerMax = 100.0f;
  public override void _PhysicsProcess(double delta)
  {
    if (_owner == null || !IsInstanceValid(_owner))
      return;
    
    CheckSnap();
    
    // --- Shooting / Snapping ---
    _shootTimer += delta;
    if (_owner.IsShooting)
    {
      if (_shootTimer >= _shootTimerMax)
      {
        _shootTimer = 0.0f;
        ShootingTimerOnTimeout();
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
    PlayerProjectile.ResetCounter();
    
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
    if (_owner == null || !IsInstanceValid(_owner))
      return;

    if (Snapped)
    {
      if (_owner.ShootingVector != Vector2.Zero)
      {
        var targetPos = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer);
        var rotationDirection = _owner.GlobalPosition.DirectionTo(targetPos);
        var targetRotation = rotationDirection.Angle();
        Rotation = targetRotation;
        GlobalPosition = targetPos;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0;
      }
      // Needed because of call order
      if (_teleportTargetPos is not null)
      {
        _teleportTargetPos = null;
      }
      return;
    }

    if (_teleportTargetPos is not null && _teleportTargetPos.Value != GlobalPosition)
    {
      GlobalPosition = _teleportTargetPos.Value;
      LinearVelocity = Vector2.Zero;
      AngularVelocity = 0;
      _teleportTargetPos = null;
    }

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
    var newShootTimerMax = 1 / _owner.Stats.FireRate;

    // Upon more or less significant change to fire rate, let him fire the first shot fast
    if (Math.Abs(newShootTimerMax - _shootTimerMax) > 0.3f && newShootTimerMax > 2.0f)
    {
      _shootTimer = newShootTimerMax - 2.0f;
    }
    _shootTimerMax = newShootTimerMax;
  }

  private Vector2? _teleportTargetPos;
  public void TeleportNextFrame(Vector2 targetPos)
  {
    _teleportTargetPos = targetPos;
  }

  private void ShootingTimerOnTimeout()
  {
    if (!_owner.IsShooting)
    {
      return;
    }

    // if (_tween != null && _tween.IsRunning())
    // {
    //   _tween.Kill();
    //   Shoot();
    // }

    _tween = GetTree().CreateTween();
    _tween.TweenProperty(StaffCore, "modulate:a", 1, 0.2f).SetTrans(Tween.TransitionType.Sine);
    _tween.TweenCallback(Callable.From(Shoot));
    _tween.TweenProperty(StaffCore, "modulate:a", 0, _shootTimerMax / 2.0f)
      .SetTrans(Tween.TransitionType.Sine);
  }
  
  private void Shoot()
  {
    if (_owner.ShootingVector.Length() < 0.01f)
    {
      // Don't shoot if owner has stopped shooting, but we already initiated shooting process
      _shootTimer = _shootTimerMax;
      return;
    }
    PackedScene projectileScene;
    if (_owner.Modifiers.IsBlargh)
    {
      projectileScene = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Blargh];
    }
    else if (_owner.Modifiers.IsFireball)
    {
      projectileScene = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Fireball];
    }
    else
    {
      projectileScene = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Trailing];
    }
    var projectile = projectileScene.Instantiate<PlayerProjectile>();
    projectile.GlobalPosition = GlobalPosition + (_owner.ShootingVector * ShootDistanceFromStaff);
    projectile.Direction = _owner.ShootingVector;
    var targetRotation = projectile.Direction.Angle();
    projectile.Rotation = targetRotation;
    GetTree().Root.AddChild(projectile);
    _owner.SpellBook.NotifyOfShot();
    projectile.Fire(_owner, Room.Current);
  }

  private void AreaOnBodyEntered(Node2D body)
  {
    if (body is not player.PlayableCharacter)
      return;
    
    _staffOverlappingWithPlayer = true;
  }

  private void AreaOnBodyExited(Node2D body)
  {
    if (body is not player.PlayableCharacter)
      return;

    _staffOverlappingWithPlayer = false;
  }

  public void PickupAreaOnBodyEntered(Node2D body)
  {
    if (_owner != null || body is not player.PlayableCharacter playableCharacter)
      return;
    
    _owner = playableCharacter;
    _owner.StatsChanged += OwnerOnStatsChanged;
    if (!IsInstanceValid(Level.Current))
    {
      GD.PushWarning("Level is null, cannot set active staff");
    }
    else
    {
      LevelManager.Instance.CurrentLevel.ActiveStaff = this;
    }
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
    TrailingForce = stats.Speed / 5.0f;
    UpdateFireRate();
  }

  private void CheckSnap()
  {
    if (_owner.IsShooting)
    {
      var targetVec = _owner.GlobalPosition + (_owner.ShootingVector * DistanceFromPlayer) - GlobalPosition; // ;
      var distance = targetVec.Length();
      
      if (!Snapped && distance < 4.0f)
      {
        SetSnap(true);
      }
    }
    else if (Snapped)
    {
      SetSnap(false);
    }
  }

  private void SetSnap(bool snapEnabled)
  {
    if (snapEnabled)
    {
      PinJoint.NodeB = _owner.GetPath();
      Snapped = true;
    }
    else
    {
      PinJoint.NodeB = null;
      Snapped = false;
    }
  }
}