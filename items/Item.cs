#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // Item.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Flamme.common.assets;
using Flamme.common.enums;
using Flamme.common.eventArgs;
using Godot;
using Godot.Collections;
using System;
using System.Net.Mime;

namespace Flamme.items;

public class Item
{
  public enum Tier
  {
    Common, // The most-of-the-time bad ones
    Uncommon, // The useless ones
    Rare, // Mostly stat ups or gameplay changing but not that good ones
    Legendary, // All/High Stats Up or good gample changes
    Exotic // Game-Changing
  }

  public uint Id { get; init; }
  public string Name { get; init; }
  public string Description { get; init; }
  public Texture2D SpriteFull { get; private set; }
  public Texture2D SpriteInventory { get; private set; } // Shows in inventory. TODO: If empty, use full path

  public Tier ItemTier { get; private set; }

  public event EventHandler ItemPickedUp;
  public event EventHandler ItemRemoved;

  private static readonly object IdCountLock = new object();
  public static uint IdCount => _idCount;
  private static uint _idCount = 0;

  public readonly Dictionary<StatType, int> StatsUpDict = new Dictionary<StatType, int>();

  public Item(string name, string description, Tier tier)
  {
    lock (IdCountLock)
    {
      Id = _idCount;
      _idCount++;
    }
    Name = name;
    Description = description;
    ItemTier = tier;
  }
  
  public Item AddSpriteFull(AssetManager.Asset asset, Vector2I atlasCoords)
  {
    SpriteFull = AssetManager.Instance.GetAtlasTexture(asset, atlasCoords);
    return this;
  }
  
  public Item AddSpriteInventory(Texture2D spriteInventory = null)
  {
    SpriteInventory = spriteInventory ?? SpriteFull;
    return this;
  }

  public Item AddStatUp(StatType statType, int count)
  {
    StatsUpDict.Add(statType, count);
    return this;
  }
  
  public void InvokePickupEvent(PlayableCharacter playableCharacter)
  {
    ItemPickedUp?.Invoke(this, new ItemChangeEventArgs(playableCharacter));
  }

  public void InvokeRemoveEvent(PlayableCharacter playableCharacter)
  {
    ItemRemoved?.Invoke(this, new ItemChangeEventArgs(playableCharacter));
  }
}