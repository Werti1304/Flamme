using Flamme.common.enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

  /// <summary>
  /// This function will always find the item, even if it's been removed from pools
  /// </summary>
  /// <param name="id">id of item</param>
  /// <param name="removeFromPools">whether to remove it from loot pools</param>
  /// <returns></returns>
  public Item GetFromId(uint id, bool removeFromPools = true)
  {
    if (removeFromPools)
    {
      foreach (var itemDict in _lootPoolItemDict)
      {
        itemDict.Value.Remove(id);
      }
    }
    return _itemDict[id];
  }

  /// <summary>
  /// This function will always find the item, even if it's been removed from pools
  /// </summary>
  /// <param name="itemName">name of item</param>
  /// <param name="removeFromPools">whether to remove it from loot pools</param>
  /// <returns></returns>
  public Item GetFromName(string itemName, bool removeFromPools = true)
  {
    var item = (from itemPair in _itemDict where itemPair.Value.Name == itemName select itemPair.Value).FirstOrDefault();

    if (item == null)
    {
      GD.Print($"Tried to get item by name, but was not found: {itemName}");
      return null;
    }

    if (removeFromPools)
    {
      foreach (var itemDict in _lootPoolItemDict)
      {
        itemDict.Value.Remove(item.Id);
      }
    }
    return item;
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