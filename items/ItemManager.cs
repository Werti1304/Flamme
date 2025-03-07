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
  private readonly Dictionary<ItemId, Item> _itemDict = new Dictionary<ItemId, Item>();
  
  // Stores list of item (ids) of every loot table
  // Gets smaller as items are less
  private readonly Dictionary<ItemLootPool, List<ItemId>> _lootPoolItemDict = new Dictionary<ItemLootPool, List<ItemId>>();

  // When there is no other item in the loot pool, pick this
  public Item DefaultItem;

  public void RegisterItem(Item item, params ItemLootPool[] lootPools)
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
    foreach (ItemLootPool lootPool in Enum.GetValues(typeof(ItemLootPool)))
    {
      _lootPoolItemDict[lootPool] = new List<ItemId>();
    }
  }

  public void SetDefaultItem(Item item)
  {
    DefaultItem = item;
  }

  public Item GetRandomFromPool(ItemLootPool itemLootPool, bool removeFromPools = true)
  {
    var lootPoolItemCount = _lootPoolItemDict[itemLootPool].Count;
    if (lootPoolItemCount == 0)
    {
      return DefaultItem;
    }
    
    var randomIndex = GD.RandRange(0, lootPoolItemCount - 1);
    GD.Print($"Random index: {randomIndex}");
    var itemId = _lootPoolItemDict[itemLootPool][randomIndex];

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
  public Item GetFromId(ItemId id, bool removeFromPools = true)
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