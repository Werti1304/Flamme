using System;
using System.Collections.Generic;
using Godot;
// ReSharper disable InvertIf

namespace Flamme.testing;

public partial class SimpleCharacter : Node2D
{
	[Export] public float SpeedMultiplier = 25.0f;
	[Export] public float FrictionMultiplier = .1f;
  [Export] public float MaxSpeed = 150.0f;
  [Export] public float MaxSpeedStaff = 300.0f;
  [Export] public float StaffDistanceFromPlayer = 20.0f;
	
	[ExportCategory("Meta")] 
  [Export] public CharacterBody2D Body;
  [Export] public RigidBody2D Staff;
  [Export] public Area2D StaffArea;
  [Export] public Sprite2D CharSprite;
  [Export] public AtlasTexture TextureCharUp;
  [Export] public AtlasTexture TextureCharDown;
  [Export] public AtlasTexture TextureCharLeft;
  [Export] public AtlasTexture TextureCharRight;

  private enum Facing
  {
    Up,
    Down,
    Left,
    Right
  }

  private readonly Dictionary<Facing, Const.InputMap.Action> _facingActionDict = new Dictionary<Facing, Const.InputMap.Action>()
  {
    { Facing.Up, Const.InputMap.Action.MoveUp },
    { Facing.Down, Const.InputMap.Action.MoveDown },
    { Facing.Left, Const.InputMap.Action.MoveLeft },
    { Facing.Right, Const.InputMap.Action.MoveRight },
  };
  
  private readonly Dictionary<Facing, Vector2> _facingNormVecDict = new Dictionary<Facing, Vector2>()
  {
    { Facing.Up, Vector2.Up },
    { Facing.Down, Vector2.Down },
    { Facing.Left, Vector2.Left },
    { Facing.Right, Vector2.Right },
  };

  private Dictionary<Facing, AtlasTexture> _facingCharTextureDict;
  
	public readonly List<Const.InputMap.Action> CurrentActions = new List<Const.InputMap.Action>();

  private Facing _currentFacing = Facing.Down;

  // Called when the node enters the scene tree for the first time.
	public override void _Ready()
  {
    _facingCharTextureDict = new Dictionary<Facing, AtlasTexture>()
    {
      { Facing.Up, TextureCharUp },
      { Facing.Down, TextureCharDown },
      { Facing.Left, TextureCharLeft },
      { Facing.Right, TextureCharRight }
    };
      
		ExportMetaNonNull.Check(this);
	}

  public override void _UnhandledKeyInput(InputEvent @event)
  {
    // Iterate over each possible action
    foreach (var pair in Const.InputMap.ActionInputDict)
    {
      if (@event.IsActionPressed(pair.Value))
      {
        CurrentActions.Add(pair.Key);
        GetViewport().SetInputAsHandled();
        break;
      }

      if (@event.IsActionReleased(pair.Value))
      {
        CurrentActions.Remove(pair.Key);
        GetViewport().SetInputAsHandled();
        break;
      }
    }

    // Efficiency increase possible here
    var newFacing = _currentFacing;
    if (CurrentActions.Contains(_facingActionDict[Facing.Down]))
    {
      newFacing = Facing.Down;
    }
    else if (CurrentActions.Contains(_facingActionDict[Facing.Up]))
    {
      newFacing = Facing.Up;
    }
    else if (CurrentActions.Contains(_facingActionDict[Facing.Left]))
    {
      newFacing = Facing.Left;
    }
    else if (CurrentActions.Contains(_facingActionDict[Facing.Right]))
    {
      newFacing = Facing.Right;
    }

    if (newFacing != _currentFacing)
    {
      _currentFacing = newFacing;
      CharSprite.Texture = _facingCharTextureDict[_currentFacing];
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    foreach (var action in CurrentActions)
    {
      Body.Velocity += action switch
      {
        Const.InputMap.Action.MoveUp => Vector2.Up * SpeedMultiplier,
        Const.InputMap.Action.MoveDown => Vector2.Down * SpeedMultiplier,
        Const.InputMap.Action.MoveLeft => Vector2.Left * SpeedMultiplier,
        Const.InputMap.Action.MoveRight => Vector2.Right * SpeedMultiplier,
        Const.InputMap.Action.ShootUp => throw new ArgumentOutOfRangeException(),
        Const.InputMap.Action.ShootDown => throw new ArgumentOutOfRangeException(),
        Const.InputMap.Action.ShootRight => throw new ArgumentOutOfRangeException(),
        Const.InputMap.Action.ShootLeft => throw new ArgumentOutOfRangeException(),
        Const.InputMap.Action.None => throw new ArgumentOutOfRangeException(),
        _ => throw new ArgumentOutOfRangeException()
      };
    }
    Body.Velocity = Body.Velocity.LimitLength(MaxSpeed);
    Body.Velocity = Body.Velocity.Lerp(Vector2.Zero, FrictionMultiplier);
    Body.MoveAndSlide();
    
    var targetVec = Body.GlobalPosition + (_facingNormVecDict[_currentFacing] * StaffDistanceFromPlayer) - Staff.GlobalTransform.Origin;
    var direction = targetVec.Normalized();
    var distance = targetVec.Length();
    Staff.ApplyCentralForce(direction * Mathf.Clamp(distance, 0, 10) * 300);
    Staff.LinearVelocity = Staff.LinearVelocity.LimitLength(MaxSpeedStaff);
    Staff.LinearVelocity = Staff.LinearVelocity.Lerp(Vector2.Zero, FrictionMultiplier);

    if (StaffArea.GetOverlappingBodies().Count == 0)
    {
      Staff.AngularVelocity = Mathf.LerpAngle(Staff.GlobalRotation, 0, 2);
    }
  }
}