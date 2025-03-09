using Flamme.common.constant;
using System;
using System.Linq;
using Flamme.common.enums;
using Flamme.items;
using Godot;
using System.Collections.Generic;

namespace Flamme.entities.player;

public partial class PlayerStats : Node2D
{
  // TODO Include Staff Stats in all calculations.. yaaay
  [Export] public int BaseHealthContainers = 3;
  [Export] public int StartingHealth = 12; // 1 health is 1/4 of a container
  [Export] public int StartingAbsorption = 12;
  
  [Export] public int BaseSpeed = 100; // px/sec?
  [Export] public float BaseDamage = 3; // Damage against enemies
  [Export] public float BaseDamageMultiplier = 1;
  [Export] public int BaseFireRate = 5; // /sec?
  [Export] public int BaseFireRateMultiplier = 1;
  [Export] public int BaseRange = 5; // In tiles
  [Export] public float BaseFireMultiplier = 1; // /min?
  [Export] public int BaseShotSpeed = 1; // px/sec
  [Export] public int BaseShotSize = 6; // px radius? Different bullets for different sizes // TODO 2 Implement using muliple sprites
  [Export] public int BaseLuck = 3; // Chance for more loot / better items
  [Export] public int BaseMana = 100; // Idk yet

  public int HealthContainers { get; private set; }
  public int NormalHealth { get; private set; }
  public int AbsorptionHealth { get; private set; }
  public int Speed { get; private set; }
  public int Range { get; private set; }
  public float Damage { get; private set; }
  public float FireRate { get; private set; } // [1-...]
  public int ShotSpeed { get; private set; }
  public int ShotSize { get; private set; }
  public int Luck { get; private set; }
  public int Mana { get; private set; }

  private readonly Godot.Collections.Dictionary<StatType, int> _statSumDict = new Godot.Collections.Dictionary<StatType, int>();

  public override void _Ready()
  {
    foreach (StatType statType in Enum.GetValues(typeof(StatType)))
    {
      _statSumDict[statType] = 0;
    }
    
    // Absorption Hearts have to be added manually
    CalculateHealthContainers();
    AddHealth(HealthType.Normal, StartingHealth);
    AddHealth(HealthType.Absorption, StartingAbsorption);
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
    
    CalculateHealthContainers();
    
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

  public void AddHealth(Dictionary<HealthType, int> healthDict)
  {
    foreach (var health in healthDict)
    {
      AddHealth(health.Key, health.Value);
    }
  }

  /// <summary>
  /// Adds health points to heart containers
  /// </summary>
  /// <param name="type">Type of added health</param>
  /// <param name="health">health points to be added</param>
  /// <returns>Whether or not it was at least partially able to add to health
  /// -> means normally if false, don't let player pick up</returns>
  public bool AddHealth(HealthType type, int health)
  {
    // Makes sure that we don't go over our max possible health
    health = Math.Min(health, Universal.MaxPlayerHealth - GetTotalHealthPoints());

    if (health == 0)
    {
      return false;
    }
    
    switch (type)
    {
      case HealthType.Normal:
        if (NormalHealth == HealthContainers * Universal.HealthPerContainer)
        {
          return false;
        }
        NormalHealth = Mathf.Min(NormalHealth + health, HealthContainers * Universal.HealthPerContainer);
        LimitHealthToBounds();
        return true;
      case HealthType.Absorption:
        health = Math.Min(health, Universal.MaxPlayerHealth - HealthContainers * Universal.HealthPerContainer);
        AbsorptionHealth += health;
        LimitHealthToBounds();
        return true;
      default:
        throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
  }

  private int GetTotalHealthPoints()
  {
    return NormalHealth + AbsorptionHealth;
  }

  /// <summary>
  /// Removes health points, takes absorption hearts into account
  /// </summary>
  /// <param name="health">health points to be removed</param>
  /// <returns> Whether or not the health was fully removed
  /// -> means its false if player died</returns>
  public bool RemoveHealth(int health)
  {
    if (health <= 0)
    {
      GD.PushWarning($"Tried to remove [{health}] health!");
    }
    AbsorptionHealth -= health;

    if (AbsorptionHealth > 0)
      return true;
    // Looks confusing but just calculates damage against normal health, with health = 0 as min
    NormalHealth = Mathf.Max(NormalHealth + AbsorptionHealth, 0);
    AbsorptionHealth = 0;
    return NormalHealth != 0;
  }
  
  private void CalculateSpeed()
  {
    Speed = BaseSpeed + _statSumDict[StatType.Speed];
    Speed = Mathf.Min(Speed, 1000);
  }

  private void CalculateHealthContainers()
  {
    HealthContainers = BaseHealthContainers + _statSumDict[StatType.HealthContainer];
    HealthContainers = Math.Min(HealthContainers, Universal.MaxPlayerHealthContainers);
    LimitHealthToBounds();
  }

  private void LimitHealthToBounds()
  {
    var overLimitHealth = GetTotalHealthPoints() - Universal.MaxPlayerHealth;

    if (overLimitHealth > 0)
    {
      // Total health points over limit
      RemoveHealth(overLimitHealth);
    }

    var overLimitAbsorption = HealthContainers * 4 + AbsorptionHealth - Universal.MaxPlayerHealth;
    if (overLimitAbsorption > 0)
    {
      RemoveHealth(overLimitAbsorption);
    }
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
    FireRate =  fireRateMultiplier * Mathf.Log(BaseFireRate + _statSumDict[StatType.FireRate]);
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