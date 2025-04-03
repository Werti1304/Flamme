using Flamme.common.input;
using Flamme.spells;
using Flamme.ui;
using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flamme.entities.player;

public partial class PlayerSpellBook : Node2D
{
  [Signal] public delegate void CastedSpellsChangedEventHandler();
  
  [Export] private PlayerPurse _purse;
  
  public enum SpellState
  {
    Ready,
    OnCoolDown,
    Possible,
    Casting
  }

  public static readonly List<PlayerInputMap.Action> ValidActions = new List<PlayerInputMap.Action>()
  {
    PlayerInputMap.Action.ShootUp,
    PlayerInputMap.Action.ShootRight,
    PlayerInputMap.Action.ShootDown,
    PlayerInputMap.Action.ShootLeft
  };
  
  public ReadOnlyDictionary<Spell, SpellState> Spells => _spells.AsReadOnly();
  private readonly Dictionary<Spell, SpellState> _spells = new Dictionary<Spell, SpellState>();
  
  private readonly List<Spell> _upTimeSpells = new List<Spell>(); // Only of State Casting
  private readonly List<Spell> _upTimeWaitingForShotSpells = new List<Spell>(); // Only of State Casting
  private readonly List<Spell> _roomCooldownSpells = new List<Spell>(); // Only of State OnCoolDown

  public int ActionsNeededIdx { get; private set; }
  
  public int AvailableCrystals => _purse.Crystals;

  public override void _Ready()
  {
    AddSpell(SpellManager.Instance.GetFromId(SpellId.RapidFire));
    AddSpell(SpellManager.Instance.GetFromId(SpellId.Blargh));
    AddSpell(SpellManager.Instance.GetFromId(SpellId.DoorOpen));
    
    StopListening();
  }

  public void AddSpell(Spell spell)
  {
    _spells.Add(spell, SpellState.Ready);
    Hud.Instance.SpellDisplay.Update(this);
  }

  public void RemoveSpell(Spell spell)
  {
    _spells.Remove(spell);
    Hud.Instance.SpellDisplay.Update(this);
  }

  public bool IsListening { get; private set; }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed(PlayerInputMap.Dict[PlayerInputMap.Action.Interact])
        || @event.IsActionPressed(PlayerInputMap.Dict[PlayerInputMap.Action.Interact2]))
    {
      StartListening();
    }
    else if (@event.IsActionReleased(PlayerInputMap.Dict[PlayerInputMap.Action.Interact])
             || @event.IsActionReleased(PlayerInputMap.Dict[PlayerInputMap.Action.Interact2]))
    {
      StopListening();
    }
  }

  
  public override void _PhysicsProcess(double delta)
  {
    // Checking for spells with uptime
    for (var i = 0; i < _upTimeSpells.Count; i++)
    {
      var spell = _upTimeSpells[i];
      spell.UptimeComponent.Tick(delta);
      if (!spell.UptimeComponent.IsAlive())
      {
        spell.UptimeComponent.Reset();
        _upTimeSpells.Remove(spell);
        _spells[spell] = SpellState.OnCoolDown;

        if (spell.CooldownRoomComponent != null)
        {
          _roomCooldownSpells.Add(spell);
        }
        EmitSignal(SignalName.CastedSpellsChanged);
        Hud.Instance.SpellDisplay.Update(this);
      }
    }

    if (_upTimeSpells.Count == 0)
    {
      SetPhysicsProcess(false);
    }
  }

  private bool _pressedDown = false;
  public override void _Input(InputEvent @event)
  {
    if (!IsListening || @event is InputEventMouse)
      return;
    
    var pressedActionRet = PlayerInputMap.GetPressedAction(@event);
    if (pressedActionRet == null)
      return;

    if (!ValidActions.Contains(pressedActionRet.Value))
      return;

    // Only for controllers with inbetween values needed
    // Could be integrated better, but can't think of anything right now
    if (Main.Instance.PlayerUsingController)
    {
      var actionStr = PlayerInputMap.Dict[pressedActionRet.Value];
      if (@event.GetActionStrength(actionStr) < 0.8f)
      {
        _pressedDown = false;
        return;
      }
      else if (_pressedDown)
      {
        return;
      }
      _pressedDown = true;
    }
      
    var pressedAction = pressedActionRet.Value;
    
    foreach (var spell in _spells.Keys)
    {
      if(_spells[spell] != SpellState.Possible)
        continue;

      if (spell.ChargeCrystalCost > 0 && spell.ChargeCrystalCost > _purse.Crystals)
      {
        _spells[spell] = SpellState.Ready;
        continue;
      }
      
      if (spell.ActionsNeeded.Count > ActionsNeededIdx 
        && spell.ActionsNeeded[ActionsNeededIdx] == pressedAction)
      {
        GD.Print($"Spell {spell.Name} now at idx {ActionsNeededIdx}");
        if (ActionsNeededIdx >= spell.ActionsNeeded.Count - 1)
        {
          if(spell.ChargeCrystalCost > 0 && !_purse.TryUseCrystals(spell.ChargeCrystalCost))
            continue;
          
          // Cast spell
          GD.Print($"Spell {spell.Name} is casting!");
          _spells[spell] = SpellState.Casting;
          EmitSignal(SignalName.CastedSpellsChanged);
          StopListening();
          // If uptime should start instantly
          if (spell.UptimeComponent != null)
          {
            if (spell.StartUptimeUponShooting)
            {
              _upTimeWaitingForShotSpells.Add(spell);
            }
            else
            {
              StartUptimeTimer(spell);
            }
          }
          spell.OnCast();
          return;
        }
      }
      else
      {
        _spells[spell] = SpellState.Ready;
      }
    }

    // If no spells are possible through our pressed inputs, just lets the user try again
    if (!_spells.Any(spell => spell.Value == SpellState.Possible))
    {
      StartListening();
    }
    else
    {
      ActionsNeededIdx++;
    }
    Hud.Instance.SpellDisplay.Update(this);
  }

  private void StartUptimeTimer(Spell spell)
  {
    _upTimeSpells.Add(spell);
    SetPhysicsProcess(true);
  }

  private void StartListening()
  {
    // Start listening
    ActionsNeededIdx = 0;
    IsListening = true;
    Hud.Instance.SpellDisplay.Update(this);
    SetProcessInput(true);
    foreach (var spell in _spells.Keys)
    {
      if (_spells[spell] == SpellState.Ready)
      {
        _spells[spell] = SpellState.Possible;
      }
    }
  }

  private void StopListening()
  {
    SetProcessInput(false);
    // When done with listening, reset all possible to inactive 
    foreach (var spell in _spells.Keys)
    {
      if (_spells[spell] == SpellState.Possible)
      {
        _spells[spell] = SpellState.Ready;
      }
    }
    IsListening = false;
    ActionsNeededIdx = 0;
    Hud.Instance.SpellDisplay.Update(this);
  }

  public void RoomCleared()
  {
    for (var i = 0; i < _roomCooldownSpells.Count; i++)
    {
      var spell = _roomCooldownSpells[i];
      spell.CooldownRoomComponent.Tick();
      if (spell.CooldownRoomComponent.IsFinished())
      {
        spell.CooldownRoomComponent.Reset();
        _roomCooldownSpells.Remove(spell);
        _spells[spell] = SpellState.Ready;
        EmitSignal(SignalName.CastedSpellsChanged);
      }
    }
    Hud.Instance.SpellDisplay.Update(this);
  }

  public void NotifyOfShot()
  {
    if (_upTimeWaitingForShotSpells.Count == 0)
      return;
    
    foreach (var spell in _upTimeWaitingForShotSpells)
    {
      StartUptimeTimer(spell);
    }
    _upTimeWaitingForShotSpells.Clear();
  }
}