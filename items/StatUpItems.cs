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
      .AddStatUp(StatType.Health, 4));

    manager.RegisterItem(
      new Item(ItemId.AbsorptionToken,"Token of Absorption", "+3 Absorption Hearts", Item.Tier.Common)
      .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(1, 0))
      .AddSpriteInventory()
      .AddStatUp(StatType.Absorption, 12),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.DamageToken,"Token of Damage", "+1 Damage", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(2, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Damage, 1),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.SpeedToken,"Token of Speed", "Speeed up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(3, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Speed, 20),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.ShotSpeedToken,"Token of Shot Speed", "Shot speed up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(4, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.ShotSpeed, 30),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.FireRateToken,"Token of Fire Rate", "Fire Rate up!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(5, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.FireRate, 30),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.ShotSizeToken,"Token of Shot Size", "Shot Size up!", Item.Tier.Common)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(6, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.ShotSize, 2),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.LuckToken,"Token of Luck", "+1 Luck!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(7, 0))
        .AddSpriteInventory()
        .AddStatUp(StatType.Luck, 1),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
    
    manager.RegisterItem(
      new Item(ItemId.AllStatsUpToken,"Lucky Wheel Token", "All Stats Up!", Item.Tier.Uncommon)
        .AddSpriteFull(AssetManager.Asset.SpriteItemStatup1, new Vector2I(0, 1))
        .AddSpriteInventory()
        .AddStatUp(StatType.Health, 4).AddStatUp(StatType.Damage, 12).AddStatUp(StatType.Speed, 10),
      LootPool.Treasure, LootPool.Chest, LootPool.Boss, LootPool.Shop
    );
  }
}
