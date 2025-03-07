using System;
using System.Collections.Generic;
using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.entities.env.health;
using Flamme.entities.env.purse;
using Flamme.world;
using Flamme.world.rooms;
using Godot;
using System.Diagnostics;

namespace Flamme.entities.env.Loot;

public partial class LootGenerator
{
  // public readonly Dictionary<LootPool, List<(int, PackedScene, Area2D)>> LootPoolDict = new();
  // public readonly Dictionary<LootPool, List<(int, LootType)>> LootPoolDict = new();
  //
  // public List<(HealthType, int min, int max)> HealthPickupList;
  //
  public enum LootType
  {
    NormalHealth,
    AbsorptionHealth,
    Coin,
    Key,
    Crystal,
    NormalChest,
    LockedChest,
    MimicChest
  }

  // TODO 1 Add weights/distribution types? idk
  public struct LootMeta(LootType lootType, int generationChance, int generationTries, int worthMin, int worthMax)
  {
    public readonly LootType LootType = lootType;
    public readonly int GenerationTries = generationTries;
    public readonly int GenerationChance = generationChance;
    public readonly int WorthMin = worthMin;
    public readonly int WorthMax = worthMax;
  }
  
  public static LootPool GetDefaultLootPool(RoomType roomType)
  {
    // TODO 2 Everyone has pathway loot for now
    switch (roomType)
    {
      case RoomType.Pathway:
        return LootPool.Pathway;
      case RoomType.Spawn:
        return LootPool.Pathway;
      case RoomType.Treasure:
        return LootPool.Pathway;
      case RoomType.Shop:
        return LootPool.Pathway;
      case RoomType.Smithy:
        return LootPool.Pathway;
      case RoomType.Boss:
        return LootPool.Pathway;
      case RoomType.Secret:
        return LootPool.Pathway;
      case RoomType.Dev:
        return LootPool.Pathway;
      default:
        throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null);
    }
  }

  /// <summary>
  /// Worth describes the metadata of the spawned item:
  /// Health drops - how much they heal (1-4)
  /// Coin - How many (1-10, 20, 30)
  /// ...
  /// </summary>
  private readonly Dictionary<LootPool, List<LootMeta>> _lootPoolDict
    = new Dictionary<LootPool, List<LootMeta>>();

  public static void SpawnLootAt(List<Node2D> lootList, Vector2 globalPosition)
  {
    foreach (var loot in lootList)
    {
      // LevelManager.Instance.CurrentLevel.LootParent.AddChild(loot);
      LevelManager.Instance.CurrentLevel.LootParent.CallDeferred(Node.MethodName.AddChild, loot);
      loot.CallDeferred(Node.MethodName.SetOwner, LevelManager.Instance.CurrentLevel);
      
      // TODO 3 Calculate where to spawn loot
      loot.GlobalPosition = globalPosition;
      GD.Print($"Spawning loot: {loot.Name} at {loot.GlobalPosition}");
      GD.Print($"Current player position: {LevelManager.Instance.CurrentLevel.PlayableCharacter.GlobalPosition}.");

      if (loot is Enemy enemy)
      {
        enemy.SetActive(LevelManager.Instance.CurrentLevel.PlayableCharacter);
      }
      
      loot.SetProcessMode(Node.ProcessModeEnum.Inherit);
      loot.SetVisible(true);
    }
  }
  
  public void RegisterLoot(LootPool lootPool, LootMeta lootMeta)
  {
    _lootPoolDict[lootPool].Add(lootMeta);
  }
  
  public void RegisterLoot(LootPool lootPool, 
    List<LootMeta> lootList)
  {
    _lootPoolDict[lootPool].AddRange(lootList);
  }

  public void GenerateLoot(Room room, RoomType roomType)
  {
    GenerateLoot(room, GetDefaultLootPool(roomType));
  }

  public void GenerateLoot(Room room, LootPool lootPool)
  {
    List<Node2D> lootToSpawn = null;
    switch (lootPool)
    {
      case LootPool.Pathway:
        lootToSpawn = GemeratePathwayLoot();
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(lootPool), lootPool, null);
    }
    
    foreach (var loot in lootToSpawn)
    {
      room.AddLoot(loot);
    }
  }

  private List<Node2D> GemeratePathwayLoot()
  {
    List<Node2D> lootToSpawn = new();

    // Decides if there will be any loot at all
    var shouldSpawnAnything = GD.RandRange(0, 100) <= 100; // TODO Change to 70% or sth
    
    if(!shouldSpawnAnything) 
      return lootToSpawn;
    
    var whatToSpawn = GD.RandRange(0, 100);

    var chanceOffset = 0; // Offset so that we correctly cover the whole range
    foreach (var lootMeta in _lootPoolDict[LootPool.Pathway])
    {
      if (whatToSpawn <= lootMeta.GenerationChance + chanceOffset)
      {
        // generate and add loot to list
        // 1 time guaranteed spawn
        var worth = (int)(GD.Randi() % (lootMeta.WorthMax - lootMeta.WorthMin + 1) + lootMeta.WorthMin);
        lootToSpawn.Add(GenerateSingleLoot(lootMeta.LootType, worth));
        
        // after that, 1/3 chance for every other try
        for (var i = 1; i < lootMeta.GenerationTries; i++)
        {
          if(GD.RandRange(0, 100) > 33)
            continue;
          
          worth = (int)(GD.Randi() % (lootMeta.WorthMax - lootMeta.WorthMin + 1) + lootMeta.WorthMin);
          lootToSpawn.Add(GenerateSingleLoot(lootMeta.LootType, worth));
        }
        break;
      }
      chanceOffset += lootMeta.GenerationChance;
    }
    return lootToSpawn;
  }

  private Node2D GenerateSingleLoot(LootType lootType, int worth)
  {
    Node2D loot = null;
    switch (lootType)
    {
      case LootType.NormalHealth:
      {
        var normalHealthNode = SceneLoader.Instance[SceneLoader.Scene.HealthPickup].Instantiate<HealthPickup>();
        normalHealthNode.HealthType = HealthType.Normal;
        normalHealthNode.HealingAmount = worth;
        loot = normalHealthNode;
        break;
      }
      case LootType.AbsorptionHealth:
      {
        var absorptionHealthNode = SceneLoader.Instance[SceneLoader.Scene.HealthPickup].Instantiate<HealthPickup>();
        absorptionHealthNode.HealthType = HealthType.Absorption;
        absorptionHealthNode.HealingAmount = worth;
        loot = absorptionHealthNode;
        break;
      }
      case LootType.Coin:
      {
        var coinNode = SceneLoader.Instance[SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        coinNode.PurseContent = PurseContent.Coin;
        coinNode.Amount = worth;
        loot = coinNode;
        break;
      }
      case LootType.Key:
      {
        var keyNode = SceneLoader.Instance[SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        keyNode.PurseContent = PurseContent.Key;
        keyNode.Amount = worth;
        loot = keyNode;
        break;
      }
      case LootType.Crystal:
      {
        var crystalNode = SceneLoader.Instance[SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        crystalNode.PurseContent = PurseContent.Crystal;
        crystalNode.Amount = worth;
        loot = crystalNode;
        break;
      }
      case LootType.NormalChest:
      {
        var normalChestNode = SceneLoader.Instance[SceneLoader.Scene.Chest].Instantiate<Chest>();
        normalChestNode.Type = Chest.ChestType.Normal;
        normalChestNode.SetLoot([GenerateSingleLoot(LootType.Coin, 5)]);
        loot = normalChestNode;
        break;
      }
      case LootType.LockedChest:
      {
        var lockedChestNode = SceneLoader.Instance[SceneLoader.Scene.Chest].Instantiate<Chest>();
        lockedChestNode.Type = Chest.ChestType.Locked;
        lockedChestNode.SetItemPickupLoot(ItemLootPool.NormalChest);
        loot = lockedChestNode;
        break;
      }
      case LootType.MimicChest:
      {
        // Todo 1 replace with something better
        var mimicChestNode = SceneLoader.Instance[SceneLoader.Scene.Chest].Instantiate<Chest>();
        mimicChestNode.Type = Chest.ChestType.Mimic;
        var enemy = SceneLoader.Instance[SceneLoader.Scene.Runner].Instantiate<Runner>();
        mimicChestNode.SetLoot([enemy]);
        loot = mimicChestNode;
        break;
      }
      default:
        throw new ArgumentOutOfRangeException(nameof(lootType), lootType, null);
    } 
    Debug.Assert(loot != null);
    loot.SetProcessMode(Node.ProcessModeEnum.Disabled);
    loot.SetVisible(false);
    return loot;
  }

  // private List<Node2D> SpawnPathwayLoot(Room room)
  // {
  //   List<Node2D> lootToSpawn = new();
  //   
  //   // TODO 1 Change into a system where one can register loot with loot type, amount, random weights, etc.
  //
  //   // 2 tries to spawn a normal heart, so that sometimes 2 spawn
  //   for (int i = 0; i < 2; i++)
  //   {
  //     var heartSpawn = GD.RandRange(0, 100);
  //
  //     if (heartSpawn < 100)
  //     {
  //       var healthScene = GD.Load<PackedScene>(PathConstants.HealthPickupScenePath);
  //       var healthNode = healthScene.Instantiate<HealthPickup>();
  //       healthNode.HealthType = GD.RandRange(0, 4) <= 0 ? HealthType.Absorption : HealthType.Normal;
  //       healthNode.HealingAmount = GD.RandRange(1, 4);
  //       lootToSpawn.Add(healthNode);
  //     }
  //   }
  //
  //   return lootToSpawn;
  // }
  
  private LootGenerator()
  {
    _instance = this;

    // Initialize lists
    foreach (var lootPool in Enum.GetValues<LootPool>())
    {
      _lootPoolDict[lootPool] = [];
    }
  }

  public static LootGenerator Instance
  {
    get
    {
      lock (Padlock)
      {
        if (_instance == null)
        {
          // ReSharper disable once ObjectCreationAsStatement
          new LootGenerator();
        }
        return _instance;
      }
    }
  }

  private static LootGenerator _instance;
  private static readonly object Padlock = new object();
}