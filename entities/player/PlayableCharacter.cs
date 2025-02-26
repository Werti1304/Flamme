using Flamme.items;
using Godot;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.common.input;
using Flamme.entities.player;
using Flamme.testing;
using Flamme.ui;
using Room = Flamme.world.rooms.Room;

public partial class PlayableCharacter : CharacterBody2D
{
  [ExportGroup("Character")]
  
  [ExportSubgroup("Physics")]
  [Export] public float AccelerationFactor = 0.1f;
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

    OnItemChange();
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
    _movingVector = _movingVector.Normalized().Round();
    
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
    Move(delta);
  }

  private void OnItemChange()
  {
    Stats.Update(HeldItems);
    Hud.Instance.UpdateStats(Stats);
  }
  
  private void BodyEntered(Node2D body)
  {
    if (body is SimpleChest chest)
    {
      if (!chest.TryInteract(this))
      {
        var item = chest.PickupItem();

        if (item == null)
        {
          return;
        }

        HeldItems.Add(item);
        Hud.Instance.CollectItem(item);
        if (item.StatsUpDict.ContainsKey(StatType.Absorption))
        {
          Stats.AddAbsorptionHealth(item.StatsUpDict[StatType.Absorption]);
        }
        OnItemChange();
      }
    }
  }

  private void AreaEntered(Area2D area)
  {
    if (area is Room room)
    {
      if (GetViewport().GetCamera2D() is PlayerCamera camera)
      {
        camera.SetRoom(room);
      }
    }
  }

  private void Move(double delta)
  {
    Velocity = Velocity.Lerp(_movingVector.Round() * Stats.Speed, AccelerationFactor);
    Velocity = Velocity.LimitLength(Stats.Speed);
    // Velocity = Velocity.Lerp(Vector2.Zero, Friction);
    
    MoveAndSlide();
  }
}
