using Flamme.items;
using Godot;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.common.input;
using Flamme.entities.common;
using Flamme.entities.env.health;
using Flamme.entities.env.shop;
using Flamme.entities.player;
using Flamme.testing;
using Flamme.ui;

public partial class PlayableCharacter : CharacterBody2D, IEnemyDamagable
{
  [ExportGroup("Character")]
  
  [ExportSubgroup("Physics")]
  [Export] public float AccelerationFactor = 0.1f;
  [Export] public float Friction = .1f;
  
  [ExportGroup("Meta")] 
  [Export] public PlayerStats Stats;
  [Export] public PlayerPurse Purse;
  [Export] public PlayerSprite Sprite;
  [Export] public Area2D InteractionArea;

  [Signal]
  public delegate void StatsChangedEventHandler(PlayerStats stats);
  
  public readonly List<Item> HeldItems = new List<Item>();
  
  public bool IsShooting { get; private set; }

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    InteractionArea.BodyEntered += BodyEntered;
    InteractionArea.AreaEntered += OnAreaEntered;

    OnInvChange();
    Hud.Instance.PurseDisplay.UpdatePurse(Purse);
  }

  private Vector2 _movingVector = Vector2.Zero;
  public Vector2 ShootingVector { get; private set; } = Vector2.Zero;
  public PlayerFacing Facing { get; private set; } = PlayerFacing.Down;

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
    
    ShootingVector = Input.GetVector(
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootLeft],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootRight],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootUp],
      PlayerInputMap.Dict[PlayerInputMap.Action.ShootDown]);
    ShootingVector = ShootingVector.Normalized().Round();
    // TODO 1 Possible race condition
    IsShooting = ShootingVector.Length() > 0;

    GetViewport().SetInputAsHandled();
    var newFacing = PlayerFacingMethods.GetFacing(ShootingVector, _movingVector);
    if (newFacing == Facing) 
      return;
    
    Facing = newFacing;
    Sprite.OnFacingChange(newFacing);
  }

  public override void _PhysicsProcess(double delta)
  {
    Move(delta);
  }
  
  public void TakeDamage(int damage)
  {
    if (!Stats.RemoveHealth(damage))
    {
      // Player Death
      if (GetViewport().GetCamera2D() is PlayerCamera camera)
      {
        // Deactivate camera
        camera.SetPhysicsProcess(false);
      }
      QueueFree(); 
    }
    OnInvChange();
  }

  private void PickupItem(Item item)
  {
    HeldItems.Add(item);
    Hud.Instance.CollectItem(item);
    OnInvChange(item);
  }

  private void OnInvChange(Item item = null)
  {
    Stats.Update(HeldItems);
    if (item != null)
    {
      Stats.AddHealth(item.HealingDict);
    }
    EmitSignal(SignalName.StatsChanged, Stats);
    Hud.Instance.UpdateStats(Stats);
    Hud.Instance.PurseDisplay.UpdatePurse(Purse);
    EscapeMenu.Instance.StatsDisplay.UpdateStats(Stats);
  }
  
  private void BodyEntered(Node2D body)
  {
    if (body is RigidBody2D rigidBody2D)
    {
      rigidBody2D.ApplyCentralForce(GlobalPosition.DirectionTo(rigidBody2D.GlobalPosition) * 3000.0f);
    }
    
    if (body is Chest chest)
    {
      if (chest.IsOpen)
      {
        // Simulate running against the item pickup
        if (chest.ItemPickupLoot != null && IsInstanceValid(chest.ItemPickupLoot) && chest.ItemPickupLoot.Monitorable)
        {
          OnAreaEntered(chest.ItemPickupLoot);
          SetDeferred(CharacterBody2D.PropertyName.Velocity, Velocity += (chest.GlobalPosition.DirectionTo(GlobalPosition) * 1000.0f));
        }
      }
      else
      {
        chest.Open();
        SetDeferred(CharacterBody2D.PropertyName.Velocity, Velocity += (chest.GlobalPosition.DirectionTo(GlobalPosition) * 1000.0f));
      }
    }
    else if (body is HealthPickup healthPickup)
    {
      if (Stats.AddHealth(healthPickup.HealthType, healthPickup.HealingAmount))
      {
        healthPickup.Consumed();
      }
      OnInvChange();
    }
    else if (body is Flamme.entities.env.purse.PursePickup pursePickup)
    {
      Purse.Add(pursePickup.Pickup());
      OnInvChange();
    }
  }
  
  private void OnAreaEntered(Area2D area)
  {
    if (area is Flamme.entities.env.ItemPickup itemPickup)
    {
      PickupItem(itemPickup.Pickup());
    }
    else if (area is CoinBuyable coinBuyable)
    {
      if (Purse.Coins < coinBuyable.Price) 
        return;
      
      Purse.Coins -= coinBuyable.Price;
      coinBuyable.Buy(this);
      OnInvChange();
    }
  }

  private void Move(double delta)
  {
    Velocity = Velocity.Lerp(_movingVector * Stats.Speed * 2, AccelerationFactor);
    Velocity = Velocity.LimitLength(Stats.Speed * 2);
    // Velocity = Velocity.Lerp(Vector2.Zero, Friction);
    
    MoveAndSlide();
  }
}
