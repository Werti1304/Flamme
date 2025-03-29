using Flamme.common.input;
using Flamme.spells;
using Flamme.ui;
using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Flamme.entities.player;

public partial class PlayerSpellPurse : Node2D
{
  [Signal] public delegate void CastedSpellsChangedEventHandler();
  
  public enum SpellState
  {
    Inactive,
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
  
  private readonly List<Spell> _upTimeSpells = new List<Spell>();

  public int ActionsNeededIdx { get; private set; }

  public override void _Ready()
  {
    AddSpell(SpellManager.Instance.GetFromId(SpellId.RapidFire));
    
    StopListening();
  }

  public void AddSpell(Spell spell)
  {
    _spells.Add(spell, SpellState.Inactive);
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
      // Start listening
      IsListening = true;
      Hud.Instance.SpellDisplay.Update(this);
      SetProcessInput(true);

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
        _spells[spell] = SpellState.Inactive;
        EmitSignal(SignalName.CastedSpellsChanged);
        Hud.Instance.SpellDisplay.Update(this);
      }
    }

    if (_upTimeSpells.Count == 0)
    {
      SetPhysicsProcess(false);
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (!IsListening || @event is InputEventMouse)
      return;
    
    var pressedActionRet = PlayerInputMap.GetPressedAction(@event);
    if (pressedActionRet == null)
      return;

    if (!ValidActions.Contains(pressedActionRet.Value))
      return;
    
    var pressedAction = pressedActionRet.Value;
    
    foreach (var spell in _spells.Keys)
    {
      if (_spells[spell] == SpellState.Inactive)
      {
        _spells[spell] = SpellState.Possible;
      }
      
      if(_spells[spell] != SpellState.Possible)
        continue;
      
      if (spell.ActionsNeeded.Count > ActionsNeededIdx 
        && spell.ActionsNeeded[ActionsNeededIdx] == pressedAction)
      {
        GD.Print($"Spell {spell.Name} now at idx {ActionsNeededIdx}");
        if (ActionsNeededIdx >= spell.ActionsNeeded.Count - 1)
        {
          // Cast spell
          GD.Print($"Spell {spell.Name} is casting!");
          _spells[spell] = SpellState.Casting;
          EmitSignal(SignalName.CastedSpellsChanged);
          StopListening();

          if (spell.UptimeComponent != null)
          {
            _upTimeSpells.Add(spell);
            SetPhysicsProcess(true);
          }
          return;
        }
      }
      else
      {
        _spells[spell] = SpellState.Inactive;
      }
    }

    // If no spells are possible through our pressed inputs, just lets the user try again
    if (!_spells.Any(spell => spell.Value == SpellState.Possible))
    {
      ActionsNeededIdx = 0;
    }
    else
    {
      ActionsNeededIdx++;
    }
    Hud.Instance.SpellDisplay.Update(this);
  }
  
  private void StopListening()
  {
    SetProcessInput(false);
    // When done with listening, reset all possible to inactive 
    foreach (var spell in _spells.Keys)
    {
      if (_spells[spell] == SpellState.Possible)
      {
        _spells[spell] = SpellState.Inactive;
      }
    }
    IsListening = false;
    ActionsNeededIdx = 0;
    Hud.Instance.SpellDisplay.Update(this);
  }
}