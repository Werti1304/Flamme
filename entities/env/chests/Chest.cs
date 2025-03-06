using Flamme.common.enums;
using Flamme.entities.env;
using Flamme.entities.env.Loot;
using Flamme.items;
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

  public void SetLoot(List<Node2D> loot)
  {
    ItemPickupLoot.ItemId = ItemId.None;
    _lootList = loot;
  }
  
  public void SetItemPickupLoot(ItemLootPool itemLootPool)
  {
    _lootList.Clear();
    ItemPickupLoot.RetrievelMode = ItemPickup.ItemRetrievel.FromItemPool;
    ItemPickupLoot.ItemLootPool = itemLootPool;
    ItemPickupLoot.Set();
  }

  private void SpawnLoot()
  {
    if (_lootList.Count > 0)
    {
      LootGenerator.SpawnLootAt(_lootList, GlobalPosition + new Vector2(0, 32));
    }
    else
    {
      ItemPickupLoot.ShowItem();
    }
  }
}
