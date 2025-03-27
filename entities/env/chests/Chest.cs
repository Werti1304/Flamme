using Flamme.common.enums;
using Flamme.entities.env.Loot;
using Flamme.entities.player;
using Flamme.world;
using Godot;
using System;
using System.Collections.Generic;

namespace Flamme.entities.env.chests;

public partial class Chest : RigidBody2D
{
  public enum ChestType
  {
    Normal,
    Locked,
    Mimic
  }
  
  [Export] public ChestType Type = ChestType.Normal;
  [Export] public bool IsOpen = false;
  
  [ExportGroup("Textures")]
  [Export] public AtlasTexture NormalChestClosedexture;
  [Export] public AtlasTexture NormalChestOpenTexture;
  [Export] public AtlasTexture LockedChestClosedexture;
  [Export] public AtlasTexture LockedChestOpenTexture;
  [Export] public AtlasTexture MimicChestClosedexture;
  [Export] public AtlasTexture MimicChestOpenTexture;

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public ItemPickup ItemPickupLoot = null;
  
  private List<Node2D> _lootList = new();
  // private ItemPickup _itemPickupLoot;
  
  public override void _Ready()
  {
    Sprite.Texture = Type switch
    {
      ChestType.Normal => IsOpen ? NormalChestOpenTexture : NormalChestClosedexture,
      ChestType.Locked => IsOpen ? LockedChestOpenTexture : LockedChestClosedexture,
      ChestType.Mimic => IsOpen ? MimicChestOpenTexture : MimicChestClosedexture,
      _ => throw new ArgumentOutOfRangeException()
    };

    if (ItemPickupLoot != null)
    {
      ItemPickupLoot.HideItem();
    }
  }
  
  public void GenerateLoot()
  {
    // TODO Switch case for different types of chest
    switch (Type)
    {
      case ChestType.Normal:
        if (_lootList.Count == 0)
        {
          _lootList = LootGenerator.Instance.GenerateChestLoot();
        }
        break;
      case ChestType.Locked:
        ItemPickupLoot ??= ItemPickup.GetInstance(ItemLootPool.LockedChest);
        break;
      case ChestType.Mimic:
        var enemy = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Runner].Instantiate<enemies.prison.runner.Runner>();
        _lootList.Add(enemy);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  public bool TryOpen(PlayerPurse purse)
  {
    if (IsOpen)
      return false;

    if (Type == ChestType.Locked)
    {
      if (purse.Keys < 1)
      {
        return false;
      } 
      purse.Keys--;
    }
    IsOpen = true;
    Sprite.Texture = Type switch
    {
      ChestType.Normal => NormalChestOpenTexture,
      ChestType.Locked => LockedChestOpenTexture,
      ChestType.Mimic => MimicChestOpenTexture,
      _ => throw new ArgumentOutOfRangeException()
    };
    SpawnLoot();
    return true;
  }
  
  private void SpawnLoot()
  {
    if (ItemPickupLoot != null)
    {
      // This should stay seperated, as the position of the itempickup should be differently calculated/set than the normal loot
      if (ItemPickupLoot.GetParent() == null)
      {
        CallDeferred(Node.MethodName.AddChild, ItemPickupLoot);
      }
      ItemPickupLoot.CallDeferred(Node.MethodName.SetOwner, LevelManager.Instance.CurrentLevel);
      
      ItemPickupLoot.Position = new Vector2(0, -14.0f);
      ItemPickupLoot.ShowItem(false);
    }
    else
    {
      LootGenerator.SpawnLootAt(_lootList, GlobalPosition + new Vector2(0, 24));
      _lootList.Clear();
    }
  }
  
  public override void _Notification(int what)
  {
    // NOTIFICATION_PREDELETE, Destructor-Equivalent for Nodes
    // https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-constant-notification-predelete
    if (what != NotificationPredelete)
    {
      return;
    }

    foreach (var node in _lootList)
    {
      node.QueueFree();
    }

    if (ItemPickupLoot != null && IsInstanceValid(ItemPickupLoot))
    {
      ItemPickupLoot.QueueFree();
    }
  }
}