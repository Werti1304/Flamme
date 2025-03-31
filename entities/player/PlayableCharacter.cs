using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.common.helpers;
using Flamme.common.input;
using Flamme.entities.common;
using Flamme.entities.enemies;
using Flamme.entities.env.health;
using Flamme.entities.env.shop;
using Flamme.items;
using Flamme.spells;
using Flamme.ui;
using Flamme.ui.death_screen;
using Flamme.world.doors;
using Flamme.world.rooms;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Flamme.entities.player;

public partial class PlayableCharacter : CharacterBody2D, IEnemyDamagable
{
  [ExportGroup("Character")]
  
  [ExportSubgroup("Physics")]
  [Export] public float AccelerationFactor = 0.1f;
  [Export] public float Friction = .1f;
  
  [ExportGroup("Meta")] 
  [Export] public PlayerStats Stats;
  [Export] public PlayerPurse Purse;
  [Export] public PlayerSpellBook SpellBook;
  [Export] public PlayerSprite Sprite;
  [Export] public Area2D InteractionArea;
  [Export] public Timer InvincibilityTimer;
  
  public ProjectileModifiers Modifiers = new ProjectileModifiers();

  [Signal]
  public delegate void StatsChangedEventHandler(PlayerStats stats);
  
  public readonly List<Item> HeldItems = new List<Item>();
  public readonly List<Spell> ActiveSpells = new List<Spell>();
  
  public bool IsShooting { get; private set; }

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    InteractionArea.BodyEntered += BodyEntered;
    InteractionArea.AreaEntered += OnAreaEntered;
    
    InvincibilityTimer.Timeout += () => Invincible = false;
    
    SpellBook.CastedSpellsChanged += OnInvChange;

    OnInvChange();
    Hud.Instance.PurseDisplay.UpdatePurse(Purse);
    Hud.Instance.SpellDisplay.Update(SpellBook);
    GD.Print($"Player {Name} ready, Parent: {GetParent()}, Owner: {GetOwner()}!");

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
    IsShooting = ShootingVector.Length() > 0 
                 && !(Input.IsActionPressed(PlayerInputMap.Dict[PlayerInputMap.Action.Interact]) 
                      || Input.IsActionPressed(PlayerInputMap.Dict[PlayerInputMap.Action.Interact2]));

    GetViewport().SetInputAsHandled();
    var newFacing = PlayerFacingMethods.GetFacing(ShootingVector, _movingVector);
    if (newFacing == Facing) 
      return;
    
    Facing = newFacing;
    Sprite.OnFacingChange(newFacing);
  }


  private int _stuckCounter;
  public override void _PhysicsProcess(double delta)
  {
    Move(delta);
    
    if (IsStuck())
    {
      SoftTeleport(Room.Current.MidPoint.GlobalPosition);
    }
  }

  private bool IsStuck()
  {
    // if player is presumably stuck between an enemy and something different,
    // Currently strictly for THE SLIDER
    if (InteractionArea.GetOverlappingBodies().Count >= 2 && InteractionArea.GetOverlappingBodies().OfType<enemies.prison.slider.Slider>().Any())
    {
      _stuckCounter++;

      if (_stuckCounter > 10)
      {
        _stuckCounter = 0;
        return true;
      }
    }
    else
    {
      _stuckCounter = 0;
    }
    return false;
  }

  private bool _invincible;
  private bool Invincible
  {
    get => _invincible;
    set
    {
      if (_isTeleporting)
      {
        if (value)
        {
          InvincibilityTimer.Start();
        }
        return;
      }
      if (value)
      {
        InvincibilityTimer.Start();
        Sprite.Modulate = Color.FromHtml("ff5959");
      }
      else
      {
        Sprite.Modulate = Colors.White;
      }
      _invincible = value;
    }
  }

  private bool _isTeleporting;
  private void SoftTeleport(Vector2 newGlobalPos)
  {
    if (_isTeleporting)
      return;
    _isTeleporting = true;
    _invincible = true;
    var tween = GetTree().CreateTween();
    var originalModulate = Sprite.Modulate;
    var newModulate = originalModulate;
    newModulate.A = 0.2f;
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), newModulate, 0.2f);
    tween.TweenProperty(this, Node2D.PropertyName.GlobalPosition.ToString(), newGlobalPos, 0.4f);
    tween.TweenProperty(Sprite, CanvasItem.PropertyName.Modulate.ToString(), originalModulate, 0.2f);
    tween.TweenCallback(Callable.From(AfterTeleport));
  }

  private void AfterTeleport()
  {
    _isTeleporting = false;
    Invincible = false;
  }
  
  public bool TakeDamage(int damage)
  {
    if (Invincible || DebugToggles.GodMode)
      return false;
    
    if (!Stats.RemoveHealth(damage))
    {
      if (GetViewport().GetCamera2D() is PlayerCamera camera)
      {
        // Deactivate camera
        camera.SetProcess(false);
        DeathScreen.Instance.ShowDeathScreen();
      }
      Room.Current.LeaveRoom(); // "Leave" room cuz player is dead
      QueueFree(); 
    }
    Invincible = true;
    EmitSignal(SignalName.StatsChanged, Stats);
    Hud.Instance.UpdateStats(Stats);
    return true;
  }

  private void PickupItem(Item item)
  {
    if (item == null)
      return;
    HeldItems.Add(item);
    Hud.Instance.CollectItem(item);
    OnInvChange(item);
  }
  
  // Needed for events
  private void OnInvChange()
  {
    OnInvChange(null);
  }

  private void OnInvChange(Item item)
  {
    ActiveSpells.Clear();
    foreach (var spells in SpellBook.Spells)
    {
      if (spells.Value == PlayerSpellBook.SpellState.Casting)
      {
        ActiveSpells.Add(spells.Key);
      }
    }
    
    Stats.Update(HeldItems, ActiveSpells);
    Modifiers.Update(HeldItems, ActiveSpells);
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
    
    if (body is env.chests.Chest chest)
    {
      if (chest.IsOpen)
      {
        // Simulate running against the item pickup
        if (chest.ItemPickupLoot != null && IsInstanceValid(chest.ItemPickupLoot))
        {
          OnAreaEntered(chest.ItemPickupLoot);
          SetDeferred(CharacterBody2D.PropertyName.Velocity,  (chest.GlobalPosition.DirectionTo(GlobalPosition) * 100.0f));
        }
      }
      else
      {
        if (chest.TryOpen(Purse))
        {
          SetDeferred(CharacterBody2D.PropertyName.Velocity,  (chest.GlobalPosition.DirectionTo(GlobalPosition) * 100.0f));
          OnInvChange();
        }
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
    else if (body is DoorMarker doorMarker && IsInstanceValid(doorMarker.Door)) // Door can be null here if doorMarker is disguised
    {
      doorMarker.Door.TryOpen(this);
      OnInvChange();
    }
    else if (body is Pedestal pedestal)
    {
      if (IsInstanceValid(pedestal.ItemPickupLoot))
      {
        OnAreaEntered(pedestal.ItemPickupLoot);
      }
    }
  }
  
  private void OnAreaEntered(Area2D area)
  {
    if (area is env.ItemPickup itemPickup)
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
    Velocity = Velocity.Lerp(_movingVector * Stats.Speed, AccelerationFactor);
    // Velocity = Velocity.LimitLength(Stats.Speed * 2);
    // Velocity = Velocity.Lerp(Vector2.Zero, Friction);

    if (MoveAndSlide())
    {
      for(var i = 0; i < GetSlideCollisionCount(); i++)
      {
        var col = GetSlideCollision(i);

        if (col.GetCollider() is Enemy enemy)
        {
          // enemy.Velocity += col.GetNormal() * - (1 / enemy.Weight) * 100.0f;
          enemy.Velocity += col.GetNormal() * - 15.0f;
        }
      }
    }
  }

  public void NotifyOfRoomClear(Room room, bool enemiesDefeated)
  {
    if (enemiesDefeated)
    {
      SpellBook.RoomCleared();
    }
  }
}