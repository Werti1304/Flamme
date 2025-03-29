using Flamme.common.assets;
using Flamme.common.enums;
using Flamme.common.eventArgs;
using Flamme.common.input;
using Flamme.entities.player;
using Flamme.items;
using Flamme.spells.components;
using Godot;
using System;
using System.Collections.Generic;

namespace Flamme.spells;

public class Spell
{
  public SpellId Id { get; init; }
  public string Name { get; init; }
  public string Description { get; init; }
  
  public event EventHandler SpellPickedUp;
  public event EventHandler SpellRemoved;
  
  public readonly List<PlayerInputMap.Action> ActionsNeeded = new List<PlayerInputMap.Action>();

  public readonly Dictionary<StatType, int> StatsUpDict = new Dictionary<StatType, int>();
  public readonly Dictionary<HealthType, int> HealingDict = new Dictionary<HealthType, int>();
  
  public readonly List<ProjectileModifiers.Modifier> ProjectileModifiers = [];
  
  public UptimeComponent UptimeComponent = null;
  public int CastTime = -1;
  
  // public int CooldownTime = -1; Maybe later
  public int CooldownRooms = -1;
  public int MagicCost = -1;

  public Spell(SpellId id, string name, string description, params PlayerInputMap.Action[] actionsNeeded)
  {
    Id = id;
    Name = name;
    Description = description;
    ActionsNeeded.AddRange(actionsNeeded);
  }

  public Spell AddStatUp(StatType statType, int count)
  {
    StatsUpDict.Add(statType, count);
    return this;
  }

  public Spell AddHealing(HealthType healthType, int health)
  {
    HealingDict.Add(healthType, health);
    return this;
  }

  public Spell AddModifier(ProjectileModifiers.Modifier modifier)
  {
    ProjectileModifiers.Add(modifier);
    return this;
  }

  public Spell SetUptime(double uptime)
  {
    UptimeComponent = new UptimeComponent(uptime);
    return this;
  }
  
  public void InvokePickupEvent(entities.player.PlayableCharacter playableCharacter)
  {
    SpellPickedUp?.Invoke(this, new SpellChangeEventArgs(playableCharacter));
  }

  public void InvokeRemoveEvent(entities.player.PlayableCharacter playableCharacter)
  {
    SpellRemoved?.Invoke(this, new SpellChangeEventArgs(playableCharacter));
  }
}
