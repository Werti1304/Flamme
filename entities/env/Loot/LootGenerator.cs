using System;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.entities.env.health;
using Flamme.entities.env.purse;
using Flamme.world;
using Godot;
using System.Diagnostics;

namespace Flamme.entities.env.Loot;

public partial class LootGenerator
{
  public static RandomNumberGenerator NotSeedRng = new RandomNumberGenerator();

  // TODO 1 Add weights/distribution types? idk
  public struct LootMeta(LootType lootType, int generationChance, int generationTries, int worthMin, int worthMax)
  {
    public readonly LootType LootType = lootType;
    public readonly int GenerationTries = generationTries;
    public readonly int GenerationChance = generationChance;
    public readonly int WorthMin = worthMin;
    public readonly int WorthMax = worthMax;
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
    bool firstSpawn = true;
    foreach (var loot in lootList)
    {
      SpawnLootAt(loot, globalPosition, !firstSpawn);
      firstSpawn = false;
    }
  }

  public static void SpawnLootAt(Node2D loot, Vector2 globalPosition, bool randomizePositionSlightly)
  {
    // LevelManager.Instance.CurrentLevel.LootParent.AddChild(loot);
    LevelManager.Instance.CurrentLevel.LootParent.CallDeferred(Node.MethodName.AddChild, loot);
    loot.CallDeferred(Node.MethodName.SetOwner, LevelManager.Instance.CurrentLevel);
      
    if (!randomizePositionSlightly)
    {
      // Randomize position by a little bit
      loot.GlobalPosition = globalPosition;
    }
    else
    {
      var randomOffset = new Vector2(NotSeedRng.RandiRange(-8, 8), NotSeedRng.RandiRange(-8, 8));
      loot.GlobalPosition = globalPosition + randomOffset;
    }
      
    GD.Print($"Spawning loot: {loot.Name} at {loot.GlobalPosition}");
    if (LevelManager.Instance.CurrentLevel.PlayableCharacter != null)
    {
      GD.Print($"Current player position: {LevelManager.Instance.CurrentLevel.PlayableCharacter.GlobalPosition}.");
    }

    if (loot is enemies.Enemy enemy)
    {
      enemy.SetActive(LevelManager.Instance.CurrentLevel.PlayableCharacter);
    }
      
    loot.SetProcessMode(Node.ProcessModeEnum.Inherit);
    loot.SetVisible(true);
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

  public Node2D GenerateLyingAroundLoot()
  {
    var whatToSpawn = GD.RandRange(0, 100);
    var chanceOffset = 0; // Offset so that we correctly cover the whole range
    foreach (var lootMeta in _lootPoolDict[LootPool.LyingAround])
    {
      if (whatToSpawn <= lootMeta.GenerationChance + chanceOffset)
      {
        // 1 time guaranteed spawn
        var worth = (int)(GD.Randi() % (lootMeta.WorthMax - lootMeta.WorthMin + 1) + lootMeta.WorthMin);
        return GenerateSingleLoot(lootMeta.LootType, worth);
      }
      chanceOffset += lootMeta.GenerationChance;
    }
    GD.PrintErr("No loot found in LyingAround loot pool");
    return null;
  }

  public List<Node2D> GenerateChestLoot()
  {
    // This should only be called if chest already decided that something from loot pool should be spawned
    
    List<Node2D> lootToSpawn = new();

    // Max 5 things, with 1/3 chance that it stops (apart from first)
    for (var i = 0; i < 5; i++)
    {
      var whatToSpawn = GD.RandRange(0, 100);
      var chanceOffset = 0; // Offset so that we correctly cover the whole range
      foreach (var lootMeta in _lootPoolDict[LootPool.LockedChest])
      {
        if (whatToSpawn <= lootMeta.GenerationChance + chanceOffset)
        {
          // generate and add loot to list
          // 1 time guaranteed spawn
          var worth = (int)(GD.Randi() % (lootMeta.WorthMax - lootMeta.WorthMin + 1) + lootMeta.WorthMin);
          lootToSpawn.Add(GenerateSingleLoot(lootMeta.LootType, worth));
        
          // after that, 1/3 chance for every other try
          for (var k = 1; k < lootMeta.GenerationTries; k++)
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

      if (GD.RandRange(1, 4) == 4)
      {
        break; // 1/4 chance that it stops
      }
    }
    
    return lootToSpawn;
  }

  public List<Node2D> GeneratePathwayLoot()
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
  
  
  

  private static Node2D GenerateSingleLoot(LootType lootType, int worth)
  {
    Node2D loot = null;
    switch (lootType)
    {
      case LootType.NormalHealth:
      {
        var normalHealthNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.HealthPickup].Instantiate<HealthPickup>();
        normalHealthNode.HealthType = HealthType.Normal;
        normalHealthNode.HealingAmount = worth;
        loot = normalHealthNode;
        break;
      }
      case LootType.AbsorptionHealth:
      {
        var absorptionHealthNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.HealthPickup].Instantiate<HealthPickup>();
        absorptionHealthNode.HealthType = HealthType.Absorption;
        absorptionHealthNode.HealingAmount = worth;
        loot = absorptionHealthNode;
        break;
      }
      case LootType.Coin:
      {
        var coinNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        coinNode.PurseContent = PurseContent.Coin;
        coinNode.Amount = worth;
        loot = coinNode;
        break;
      }
      case LootType.Key:
      {
        var keyNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        keyNode.PurseContent = PurseContent.Key;
        keyNode.Amount = worth;
        loot = keyNode;
        break;
      }
      case LootType.Crystal:
      {
        var crystalNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.PursePickup].Instantiate<PursePickup>();
        crystalNode.PurseContent = PurseContent.Crystal;
        crystalNode.Amount = worth;
        loot = crystalNode;
        break;
      }
      case LootType.NormalChest:
      {
        var normalChestNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Chest].Instantiate<chests.Chest>();
        normalChestNode.Type = chests.Chest.ChestType.Normal;
        normalChestNode.GenerateLoot();
        loot = normalChestNode;
        break;
      }
      case LootType.LockedChest:
      {
        var lockedChestNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Chest].Instantiate<chests.Chest>();
        lockedChestNode.Type = chests.Chest.ChestType.Locked;
        lockedChestNode.GenerateLoot();
        loot = lockedChestNode;
        break;
      }
      case LootType.MimicChest:
      {
        // Todo 1 replace with something better
        var mimicChestNode = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Chest].Instantiate<chests.Chest>();
        mimicChestNode.Type = chests.Chest.ChestType.Mimic;
        mimicChestNode.GenerateLoot();
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