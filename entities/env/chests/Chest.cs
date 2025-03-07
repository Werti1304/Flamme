using Flamme.common.enums;
using Flamme.entities.env;
using Flamme.entities.env.Loot;
using Flamme.items;
using Flamme.world;
using Godot;
using System;
using System.Collections.Generic;

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
  [Export] public ItemPickup ItemPickupLoot;
  
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
  }

  public void Open()
  {
    if (IsOpen)
      return;
    IsOpen = true;
    Sprite.Texture = Type switch
    {
      ChestType.Normal => NormalChestOpenTexture,
      ChestType.Locked => LockedChestOpenTexture,
      ChestType.Mimic => MimicChestOpenTexture,
      _ => throw new ArgumentOutOfRangeException()
    };
    SpawnLoot();
  }

  public void SetLoot(List<Node2D> lootList)
  {
    ItemPickupLoot.ItemId = ItemId.None;
    _lootList = lootList;
  }
  
  public void SetItemPickupLoot(ItemLootPool itemLootPool)
  {
    foreach (var loot in _lootList)
    {
      loot.QueueFree();
    }
    _lootList.Clear();
    ItemPickupLoot.RetrievelMode = ItemPickup.ItemRetrievel.FromItemPool;
    ItemPickupLoot.ItemLootPool = itemLootPool;
    ItemPickupLoot.Set();
  }

  private void SpawnLoot()
  {
    if (_lootList.Count > 0)
    {
      // TODO 3 get current room as owner
      LootGenerator.SpawnLootAt(_lootList, GlobalPosition + new Vector2(0, 32));
      _lootList.Clear();
    }
    else
    {
      ItemPickupLoot.ShowItem();
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
  }
}
