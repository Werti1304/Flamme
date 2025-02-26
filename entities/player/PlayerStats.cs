using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Flamme.common.enums;
using Flamme.items;
using Godot;

namespace Flamme.entities.player;

public partial class PlayerStats : Node2D
{
  // TODO Include Staff Stats in all calculations.. yaaay
  [Export] public int BaseHealth = 12; // /4 = Hearts
  [Export] public int StartingHealth = 12;
  [Export] public int StartingAbsorption = 0;
  [Export] public int BaseSpeed = 200; // px/sec?
  [Export] public float BaseDamage = 3; // Damage against enemies
  [Export] public float BaseDamageMultiplier = 1;
  [Export] public int BaseFireRate = 60; // /min?
  [Export] public int BaseFireRateMultiplier = 1;
  [Export] public int BaseRange = 32; // In pixels
  [Export] public float BaseFireMultiplier = 1; // /min?
  [Export] public int BaseShotSpeed = 2; // px/sec? no
  [Export] public int BaseShotSize = 6; // px radius? Different bullets for different sizes // TODO 2 Implement using muliple sprites
  [Export] public int BaseLuck = 3; // Chance for more loot / better items
  [Export] public int BaseMana = 100; // Idk yet

  public int Health { get; private set; }
  public int HealthContainerMax { get; private set; }
  public int AbsorptionHealth { get; private set; }
  public int Speed { get; private set; }
  public int Range { get; private set; }
  public float Damage { get; private set; }
  public int FireRate { get; private set; }
  public int ShotSpeed { get; private set; }
  public int ShotSize { get; private set; }
  public int Luck { get; private set; }
  public int Mana { get; private set; }

  private readonly Dictionary<StatType, int> _statSumDict = new Dictionary<StatType, int>();

  public override void _Ready()
  {
    // Absorption Hearts have to be added manually
    Health = StartingHealth;
    HealthContainerMax = BaseHealth;
    AbsorptionHealth = StartingAbsorption;
  }

  /// <summary>
  /// Updates all public properties based on base stats of the character and items
  /// Should be called every time a stats change (item held change) occurs
  /// </summary>
  /// <param name="items">items held by the player</param>
  public void Update(IEnumerable<Item> items)
  {
    foreach (StatType statType in Enum.GetValues(typeof(StatType)))
    {
      _statSumDict[statType] = 0;
    }
    
    foreach (var statUp in items.SelectMany(item => item.StatsUpDict))
    {
      _statSumDict[statUp.Key] += statUp.Value;
    }
    
    var oldHealthMax = HealthContainerMax;
    CalculateHealth();
    // We got more health
    if (oldHealthMax < HealthContainerMax)
    {
      Health += HealthContainerMax - oldHealthMax;
    }
    else if (oldHealthMax > HealthContainerMax)
    {
      Health = HealthContainerMax;
    }
    
    // Damage has to be calculated before ShotSize!
    CalculateDamage();
    CalculateFireRate();
    CalculateSpeed();
    CalculateRange();
    CalculateShotSpeed();
    CalculateShotSize();
    CalculateLuck();
    CalculateMana();
  }

  /// <summary>
  /// Adds health points to heart containers
  /// </summary>
  /// <param name="health">health points to be added</param>
  /// <returns>Whether or not it was at least partially able to add to health
  /// -> means normally if false, don't let player pick up</returns>
  public bool AddHealth(int health)
  {
    if (Health == HealthContainerMax)
    {
      return false;
    }
    Health = Mathf.Min(Health + health, HealthContainerMax);
    return true;
  }

  /// <summary>
  /// Removes health points, takes absorption hearts into account
  /// </summary>
  /// <param name="health">health points to be removed</param>
  /// <returns> Whether or not the health could be fully removed
  /// -> means its false if player died</returns>
  public bool RemoveHealth(int health)
  {
    AbsorptionHealth -= health;

    if (AbsorptionHealth > 0)
      return true;
    Health = Mathf.Max(Health - AbsorptionHealth, 0);
    AbsorptionHealth = 0;
    return Health != 0;
  }
  
  private void CalculateSpeed()
  {
    Speed = BaseSpeed + _statSumDict[StatType.Speed];
    Speed = Mathf.Min(Speed, 1000);
  }

  public void AddAbsorptionHealth(int absorption)
  {
    AbsorptionHealth += absorption;
  }

  private void CalculateHealth()
  {
    HealthContainerMax = BaseHealth + _statSumDict[StatType.Health];
  }

  private void CalculateDamage()
  {
    var damageMultiplier = BaseDamageMultiplier + _statSumDict[StatType.DamageMultiplier];
    Damage = (float)(damageMultiplier *
              (BaseDamage + Mathf.Sqrt(_statSumDict[StatType.Damage] * 1.3 + 1) + _statSumDict[StatType.DamageFlat]));
  }

  private void CalculateFireRate()
  {
    var fireRateMultiplier = BaseFireMultiplier + _statSumDict[StatType.FireMultiplier];
    FireRate =  (int)(fireRateMultiplier * (BaseFireRate + _statSumDict[StatType.FireRate]));
    FireRate = Mathf.Min(1000, FireRate);
  }
  
  private void CalculateRange()
  {
    Range = (_statSumDict[StatType.RangeMultiplier] + 1) * (BaseRange + _statSumDict[StatType.Range]);
  }

  private void CalculateShotSpeed()
  {
    ShotSpeed = BaseShotSpeed + _statSumDict[StatType.ShotSpeed];
    ShotSpeed = Mathf.Min(400, ShotSpeed);
  }

  private void CalculateShotSize()
  {
    ShotSize = BaseShotSize + (int)(Mathf.Sqrt(Damage) - 2) + _statSumDict[StatType.ShotSize];
  }

  private void CalculateLuck()
  {
    Luck = BaseLuck + _statSumDict[StatType.Luck];
  }

  private void CalculateMana()
  {
    Mana = BaseMana + _statSumDict[StatType.Mana];
  }
}