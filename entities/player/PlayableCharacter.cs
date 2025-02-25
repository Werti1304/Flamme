using Flamme.items;
using Godot;
using System;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.common.input;
using Flamme.entities.player;
using Flamme.testing;
using Flamme.ui;

public partial class PlayableCharacter : CharacterBody2D
{
  [ExportGroup("Character")]
  
  [ExportSubgroup("Physics")]
  [Export] public float AccelerationFactor = 0.2f;
  [Export] public float Friction = .1f;
  
  [ExportGroup("Meta")] 
  [Export] public PlayerStats Stats;
  [Export] public PlayerSprite Sprite;
  [Export] public Area2D InteractionArea;
  
  public List<Item> HeldItems = new List<Item>();

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    InteractionArea.AreaEntered += AreaEntered;
    InteractionArea.BodyEntered += BodyEntered;
    
    Stats.Update(HeldItems);
  }
  
  private Vector2 _movingVector = Vector2.Zero;
  private Vector2 _shootingVector = Vector2.Zero;
  private PlayerFacing _facing = PlayerFacing.Down;

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouse)
    {
      return;
    }
    
    _movingVector = Input.GetVector(
      PlayerInputMap.Dict[PlayerInputMap.Action.MoveLeft],
      PlayerInputMap.Dict[PlayerInputMap.Action.MoveRight],
      PlayerInputMap.Dict[PlayerInputMap.Action.MoveUp],
      PlayerInputMap.Dict[PlayerInputMap.Action.MoveDown]);
    
    _shootingVector = Input.GetVector(
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootLeft],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootRight],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootUp],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootDown]);

    GetViewport().SetInputAsHandled();
    var newFacing = PlayerFacingMethods.GetFacing(_shootingVector, _movingVector);
    if (newFacing == _facing) 
      return;
    
    _facing = newFacing;
    Sprite.OnFacingChange(newFacing);
  }

  public override void _PhysicsProcess(double delta)
  {
    Move();
  }

  public void PickupItem(Item item)
  {
    HeldItems.Add(item);
    Stats.Update(HeldItems);
    Hud.Instance.UpdateHealth(Stats.HealthContainers);
  }
  
  private void BodyEntered(Node2D body)
  {

  }

  private void AreaEntered(Area2D area)
  {

  }

  private void Move()
  {
    Velocity += _movingVector * (AccelerationFactor * Stats.Speed);
    Velocity = Velocity.LimitLength(Stats.Speed);
    Velocity = Velocity.Lerp(Vector2.Zero, Friction);
    MoveAndSlide();
  }
}
