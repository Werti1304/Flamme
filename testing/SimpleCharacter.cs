using Flamme.common.enums;
using Flamme.items;
using Flamme.ui;
using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

// ReSharper disable InvertIf

namespace Flamme.testing;

public partial class SimpleCharacter : CharacterBody2D
{
  [ExportGroup("Character")]
	[Export] public float SpeedMultiplier = 25.0f;
	[Export] public float FrictionMultiplier = .1f;
  [Export] public float MaxSpeed = 150.0f;
  [Export] public float ProjectileSpawnFromPlayer = 20.0f;
  // Player hearts = health / 4 
  [Export] public int PlayerBaseHealth = 12;
  [Export] public float BulletBaseDmg = 3;
  
  [ExportGroup("Staff")]
  [Export] public float StaffDistanceFromPlayer = 15.0f;
  [Export] public float MaxSpeedStaff = 500.0f;
  [Export] public float StaffFrictionMultiplier = .1f;
  [ExportSubgroup("Snapping")]
  [Export] public float SnapForceStaff = 500.0f;
  [Export] public float StaffSnapFrictionMultiplier = .5f;
  [Export] public float AngularFrictionMultiplierStaff = .05f;
  [ExportSubgroup("Trailing")]
  [Export] public float StaffCharDistStartTrail = 50.0f;
  [Export] public float StaffCharDistStopTrail = 30.0f;
  [Export] public float TrailForceStaff = 100.0f;
  [Export] public float StaffTrailFrictionMultiplier = .1f;

  [ExportGroup("Meta")] 
  [Export] public Area2D BodyArea;
  [Export] public Camera2D Camera;
  [Export] public Staff Staff;
  [Export] public Sprite2D CharSprite;
  [Export] public PackedScene Bullet;
  [Export] public Timer BulletCooldownTimer;
  [Export] public Timer InvinciblityTimer; // After taking damage, invincibility++
  [Export] public AtlasTexture CharUpTexture;
  [Export] public AtlasTexture CharDownTexture;
  [Export] public AtlasTexture CharLeftTexture;
  [Export] public AtlasTexture CharRightTexture;

  public List<Item> HeldItems = new List<Item>();
  
  private readonly Dictionary<Const.Facing, float> _facingStaffRotationDict = new Dictionary<Const.Facing, float>()
  {
    { Const.Facing.Up, Mathf.DegToRad(40) },
    { Const.Facing.Down, Mathf.DegToRad(40) },
    { Const.Facing.Left, Mathf.DegToRad(-45) },
    { Const.Facing.Right, Mathf.DegToRad(-45) },
  };

  public readonly Dictionary<Const.Facing, Vector2I> FacingVectorDict = new Dictionary<Const.Facing, Vector2I>()
  {
    { Const.Facing.Up, Vector2I.Up },
    { Const.Facing.Down, Vector2I.Down },
    { Const.Facing.Left, Vector2I.Left },
    { Const.Facing.Right, Vector2I.Right }
  };

  private Dictionary<Const.Facing, AtlasTexture> _facingCharTextureDict;

  private Const.InputMap.Action _currentActionsBmap = Const.InputMap.Action.None;

  private Dictionary<Const.InputMap.Action, Action> _actionInputActionDict;

  public Const.Facing CurrentFacing = Const.Facing.Down;

  private bool _isTrailing = false;
  private bool _isBulletOnCooldown = false;

  public bool IsInvincible = false;

  // Called when the node enters the scene tree for the first time.
	public override void _Ready()
  {
    // Needs to be here
    _facingCharTextureDict = new Dictionary<Const.Facing, AtlasTexture>()
    {
      { Const.Facing.Up, CharUpTexture },
      { Const.Facing.Down, CharDownTexture },
      { Const.Facing.Left, CharLeftTexture },
      { Const.Facing.Right, CharRightTexture }
    };
    
    _actionInputActionDict = new Dictionary<Const.InputMap.Action, Action>
    {
      { Const.InputMap.Action.MoveUp, () => Velocity += Vector2.Up * SpeedMultiplier },
      { Const.InputMap.Action.MoveDown, () => Velocity += Vector2.Down * SpeedMultiplier },
      { Const.InputMap.Action.MoveLeft, () => Velocity += Vector2.Left * SpeedMultiplier },
      { Const.InputMap.Action.MoveRight,  () => Velocity += Vector2.Right * SpeedMultiplier },
    };
    
		ExportMetaNonNull.Check(this);
    
    BulletCooldownTimer.Timeout += OnBulletCooldownTimer;
    InvinciblityTimer.Timeout += OnInvincibilityOver;
    BodyArea.AreaEntered += OnAreaEntered;
    BodyArea.BodyEntered += OnBodyEntered;

    UpdateStats();

    // Staff.IdleAnimationPlayer.Play("Staff Idle");
  }

  public int EffHealth;
  public float EffDamage;

  private void UpdateStats()
  {
    var upgradeHealth = 0;
    var upgradeDamage = 0;
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var item in HeldItems)
    {
      foreach (var statUp in item.StatsUpDict)
      {
        switch (statUp.Key)
        {
          case StatType.Health:
            upgradeHealth += statUp.Value;
            break;
          case StatType.Damage:
            upgradeDamage += statUp.Value;
            break;
          // TODO 5 Implement functionality of missing stat-up types
          // See PlayerStats for explanation on what stat should do what
          default:
            GD.PushWarning($"Stat Up Type {statUp.Key} not yet implemented!");
            break;
        }
      }
    }
    EffDamage = BulletBaseDmg + upgradeDamage;
    EffHealth = PlayerBaseHealth + upgradeHealth;
    
    Hud.Instance.UpdateHealth(EffHealth);
  }

  private void OnBodyEntered(Node2D body)
  {
    if (body is Enemy e)
    {
      TakeDamage(e.GetMeleeDamage());
    }
    else if (body is SimpleChest t)
    {
      if (!t.TryInteract(this))
      {
        var item = t.PickupItem();

        if (item == null)
        {
          return;
        }
        Hud.Instance.CollectItem(item);
        HeldItems.Add(item);
        UpdateStats();
      }
    }
  }

  private void OnAreaEntered(Area2D area)
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

  public override void _UnhandledInput(InputEvent @event)
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
    var newFacing = CurrentFacing;
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

    if (newFacing != CurrentFacing)
    {
      CurrentFacing = newFacing;
      CharSprite.Texture = _facingCharTextureDict[CurrentFacing];
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
    
    // Push rigidbodies if found
    for (int i = 0; i < GetSlideCollisionCount(); i++)
    {
      var collider = GetSlideCollision(i);
      
      if (collider.GetCollider() is RigidBody2D rigidBody2D)
      {
        rigidBody2D.ApplyCentralForce(-collider.GetNormal() * 100);
      }
      if (collider.GetCollider() is Enemy e)
      {
        e.Velocity += -collider.GetNormal() * 100;
      }
    }
    
    Velocity = Velocity.LimitLength(MaxSpeed);
    Velocity = Velocity.Lerp(Vector2.Zero, FrictionMultiplier);
    MoveAndSlide();
    
    // If actively shooting, the staff should snap to the right direction
    var isShooting = (_currentActionsBmap & (Const.InputMap.Action.ShootDown 
                                            | Const.InputMap.Action.ShootUp 
                                            | Const.InputMap.Action.ShootLeft 
                                            | Const.InputMap.Action.ShootRight)) > 0;
    
    // If too far from character, staff should trail behind, can't snap and trail at the same time though
    if (!_isTrailing && !isShooting && GlobalPosition.DistanceTo(Staff.GlobalPosition) > StaffCharDistStartTrail)
    {
      _isTrailing = true;
    }
    else if(_isTrailing && GlobalPosition.DistanceTo(Staff.GlobalPosition) < StaffCharDistStopTrail)
    {
      _isTrailing = false;
    }
    
    // Projectile stuff stuff
    if (isShooting && !_isBulletOnCooldown)
    {
      var bullet = Bullet.Instantiate<Bullet>();
      GetTree().Root.AddChild(bullet);
      bullet.GlobalPosition = GlobalPosition + (Const.FacingNormVecDict[CurrentFacing] * ProjectileSpawnFromPlayer);
      // bullet.Damage = EffDamage;
      if (Velocity.Length() > 10)
      {
        bullet.Direction = 0.6f * Const.FacingNormVecDict[CurrentFacing] + 0.4f * Velocity.Normalized();
      }
      else
      {
        bullet.Direction = Const.FacingNormVecDict[CurrentFacing];
      }
      _isBulletOnCooldown = true;
      BulletCooldownTimer.Start();
    }
      
    // Staff and snapping stuff
    if (isShooting)
    {
      var targetVec = GlobalPosition + (Const.FacingNormVecDict[CurrentFacing] * StaffDistanceFromPlayer) - Staff.GlobalTransform.Origin;
      var direction = targetVec.Normalized();
      var distance = targetVec.Length();
      Staff.ApplyCentralForce(direction * Mathf.Clamp(distance, 20, 200) * SnapForceStaff);
      Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, StaffSnapFrictionMultiplier); // Friction
    }
    else if(_isTrailing)
    {
      // Otherwise, just trail behind the player if they're too far away
      var targetVec = GlobalPosition - Staff.GlobalTransform.Origin;
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
    
    var overlapsPlayer = Staff.Area.GetOverlappingBodies().Contains(this);
    var overlapMinusPlayer = Staff.Area.GetOverlappingBodies().Count - (overlapsPlayer ? 1 : 0);
    
    if (isShooting && overlapMinusPlayer == 0 && Mathf.Abs(_facingStaffRotationDict[CurrentFacing] - Staff.Rotation) > 0.01)
    {
      var targetRotation = _facingStaffRotationDict[CurrentFacing];
      var angleDiff = Mathf.PosMod(_facingStaffRotationDict[CurrentFacing] - Staff.Rotation, Mathf.Tau);

      if (angleDiff > Mathf.Pi)
      {
        angleDiff -= Mathf.Tau;
      }
      else
      {
        angleDiff += Mathf.Tau;
      }
      Staff.AngularVelocity = angleDiff * 10;
    }
    
    // Angular friction
    Staff.AngularVelocity = Mathf.Lerp(Staff.AngularVelocity, 0, AngularFrictionMultiplierStaff);
  }

  public void TakeDamage(int damage)
  {
    if (IsInvincible)
    {
      return;
    }
    IsInvincible = true;
    InvinciblityTimer.Start();
    // calculate actual damage through idk
    PlayerBaseHealth -= damage;

    if (PlayerBaseHealth < 1)
    {
      // Game over screen
      GetTree().Quit();
    }
    Hud.Instance.UpdateHealth(PlayerBaseHealth);
  }

  public void OnInvincibilityOver()
  {
    IsInvincible = false;
    
    foreach(var body in BodyArea.GetOverlappingBodies())
    {
      if (body is Enemy e)
      {
        TakeDamage(e.GetMeleeDamage());
      }
    }
  }

  private void OnBulletCooldownTimer()
  {
    _isBulletOnCooldown = false;
  }
}