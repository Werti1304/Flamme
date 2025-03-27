using Flamme.common.assets;
using Flamme.common.enums;
using Godot;

namespace Flamme.items;

public static class StatUpItems
{
  public static void RegisterStatUpItems()
  {
    var manager = ItemManager.Instance;
    
    // TODO Add lang file & functionality for translation
    
    manager.SetDefaultItem(new Item(ItemId.HealthToken, "Token of Health", "+1 Heart", Item.Tier.Uncommon)
      .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(0, 0))
      .AddSpriteInventory()
      .AddStatUp(StatType.HealthContainer, 1)
      .AddHealing(HealthType.Normal, 8));
    
    manager.RegisterItem(
      new Item(ItemId.HealthToken, "Token of Health", "+1 Heart", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(0, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.HealthContainer, 1)
        .AddHealing(HealthType.Normal, 8),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );

    manager.RegisterItem(
      new Item(ItemId.AbsorptionToken, "Token of Absorption", "+3 Absorption Hearts", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(1, 0))
        .AddSpriteInventory()
        .AddHealing(HealthType.Absorption, 12),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.DamageToken,"Token of Damage", "+1 Damage", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(2, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Damage, 1),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.SpeedToken,"Token of Speed", "Speeed up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(3, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Speed, 50),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.ShotSpeedToken,"Token of Shot Speed", "Shot speed up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(4, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.ShotSpeed, 1),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.FireRateToken,"Token of Fire Rate", "Fire Rate up!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(5, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.FireRate, 5),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.ShotSizeToken,"Token of Shot Size", "Shot Size up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(6, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.ShotSize, 2),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.LuckToken,"Token of Luck", "+1 Luck!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(7, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Luck, 1),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.RangeToken,"Range Token", "Range Up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(0, 1))
        .AddSpriteInventory()
        .AddStatUp(StatType.Range, 4),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.AllStatsUpToken,"Lucky Wheel Token", "All Stats Up!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(1, 1))
        .AddSpriteInventory()
        .AddStatUp(StatType.HealthContainer, 1).AddStatUp(StatType.Damage, 12).AddStatUp(StatType.Speed, 10)
        .AddHealing(HealthType.Absorption, 4).AddHealing(HealthType.Normal, 4),
      ItemLootPool.Treasure, ItemLootPool.LockedChest, ItemLootPool.Boss, ItemLootPool.Shop
    );
  }
}
