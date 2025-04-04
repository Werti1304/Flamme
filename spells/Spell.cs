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
  
  public event EventHandler SpellCast;
  
  public readonly List<PlayerInputMap.Action> ActionsNeeded = new List<PlayerInputMap.Action>();

  public readonly Dictionary<StatType, float> StatsUpDict = new Dictionary<StatType, float>();
  public readonly Dictionary<HealthType, int> HealingDict = new Dictionary<HealthType, int>();
  
  public readonly List<ProjectileModifiers.Modifier> ProjectileModifiers = [];
  
  public UptimeComponent UptimeComponent = null;
  public CooldownRoomComponent CooldownRoomComponent = null;

  public int ChargeCrystalCost = 0; 
  
  public int CastTime = -1;
  
  public int MagicCost = -1;

  public bool StartUptimeUponShooting = false;

  public Spell(SpellId id, string name, string description, params PlayerInputMap.Action[] actionsNeeded)
  {
    Id = id;
    Name = name;
    Description = description;
    ActionsNeeded.AddRange(actionsNeeded);
  }

  public Spell AddStatUp(StatType statType, float count)
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

  public Spell SetUptimeStartUponShooting(bool val = true)
  {
    StartUptimeUponShooting = val;
    return this;
  }

  public Spell SetChargeCrystalCost(int chargeCrystalCost)
  {
    ChargeCrystalCost = chargeCrystalCost;
    return this;
  }

  public Spell SetCooldownRooms(int cooldownRooms)
  {
    CooldownRoomComponent = new CooldownRoomComponent(cooldownRooms);
    return this;
  }

  public void OnCast()
  {
    SpellCast?.Invoke(this, EventArgs.Empty);
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
