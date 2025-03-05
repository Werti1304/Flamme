using System;
using System.Collections.Generic;
using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.entities.env.health;
using Flamme.world.rooms;
using Godot;

namespace Flamme.entities.env.Loot;

public partial class LootGenerator : Node2D
{
  // public readonly Dictionary<LootPool, List<(int, PackedScene, Area2D)>> LootPoolDict = new();
  // public readonly Dictionary<LootPool, List<(int, LootType)>> LootPoolDict = new();
  //
  // public List<(HealthType, int min, int max)> HealthPickupList;
  //
  public enum LootType
  {
    Item,
    Health,
    Purse,
    Chest // Todo 4
  }

  public static readonly Dictionary<LootType, string> LootSceneDict = new Dictionary<LootType, string>()
  {
    { LootType.Item, PathConstants.ItemPickupScenePath },
    { LootType.Health, PathConstants.HealthPickupScenePath },
    { LootType.Purse, PathConstants.PursePickupScenePath },
  };
  
  public void RegisterLoot(LootPool lootPool, LootType lootType, int weight)
  {
    
  }

  public void SpawnLoot(Room room, LootPool lootPool)
  {
    List<Node2D> lootToSpawn = null;
    switch (lootPool)
    {
      case LootPool.Pathway:
        lootToSpawn = SpawnPathwayLoot(room);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(lootPool), lootPool, null);
    }
    
    foreach (var loot in lootToSpawn)
    {
      // TODO Somethings not working here
      room.CallDeferred(Node.MethodName.AddChild, loot);
      loot.SetDeferred(Node.PropertyName.Owner, room);
      loot.GlobalPosition = room.GetFreeLootPosition();
      GD.Print($"Spawned loot: {loot.Name} at {loot.GlobalPosition}");
    }
  }

  private List<Node2D> SpawnPathwayLoot(Room room)
  {
    List<Node2D> lootToSpawn = new();
    
    // TODO 1 Change into a system where one can register loot with loot type, amount, random weights, etc.

    // 2 tries to spawn a normal heart, so that sometimes 2 spawn
    for (int i = 0; i < 2; i++)
    {
      var heartSpawn = GD.RandRange(0, 100);

      if (heartSpawn < 100)
      {
        var healthScene = GD.Load<PackedScene>(PathConstants.HealthPickupScenePath);
        var healthNode = healthScene.Instantiate<HealthPickup>();
        healthNode.HealthType = GD.RandRange(0, 4) <= 0 ? HealthType.Absorption : HealthType.Normal;
        healthNode.HealingAmount = GD.RandRange(1, 4);
        lootToSpawn.Add(healthNode);
      }
    }

    return lootToSpawn;
  }
  
  public LootGenerator()
  {
    _instance = this;
  }

  public static LootGenerator Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }

  private static LootGenerator _instance;
  private static readonly object Padlock = new object();
}