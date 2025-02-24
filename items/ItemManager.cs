using Flamme.common.enums;
using Godot;
using System;
using System.Collections.Generic;

namespace Flamme.items;

public partial class ItemManager : Node
{
  // Stores item id and item as a dict, I don't trust an array alone
  // And Item Registration in the Item Class feels like bad form
  private readonly Dictionary<uint, Item> _itemDict = new Dictionary<uint, Item>();
  
  // Stores list of item (ids) of every loot table
  // Gets smaller as items are less
  private readonly Dictionary<LootPool, List<uint>> _lootPoolItemDict = new Dictionary<LootPool, List<uint>>();

  // When there is no other item in the loot pool, pick this
  private Item _defaultItem;

  public void RegisterItem(Item item, params LootPool[] lootPools)
  {
    _itemDict[item.Id] = item;
    foreach (var lootPool in lootPools)
    {
      _lootPoolItemDict[lootPool].Add(item.Id);
    }
  }
  
  public ItemManager()
  {
    _instance = this;

    // Adds dictionaries for every loot pool
    foreach (LootPool lootPool in Enum.GetValues(typeof(LootPool)))
    {
      _lootPoolItemDict[lootPool] = new List<uint>();
    }
  }

  public void SetDefaultItem(Item item)
  {
    _defaultItem = item;
  }

  public Item GetRandomFromPool(LootPool lootPool, bool removeFromPools = true)
  {
    var lootPoolItemCount = _lootPoolItemDict[lootPool].Count;
    if (lootPoolItemCount == 0)
    {
      return _defaultItem;
    }
    
    var randomIndex = GD.RandRange(0, lootPoolItemCount - 1);
    GD.Print($"Random index: {randomIndex}");
    var itemId = _lootPoolItemDict[lootPool][randomIndex];

    // ReSharper disable once InvertIf
    if (removeFromPools)
    {
      foreach (var itemDict in _lootPoolItemDict)
      {
        itemDict.Value.Remove(itemId);
      }
    }
    return _itemDict[itemId];
  }
  
  public static ItemManager Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static ItemManager _instance;
  private static readonly object Padlock = new object();
}