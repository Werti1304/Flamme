#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // ModifierItems.cs
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
using Flamme.entities.player;
using Godot;
using System.IO;

namespace Flamme.items;

public class ModifierItems
{
  public static void RegisterItems()
  {
    var manager = ItemManager.Instance;
    
    // TODO Add lang file & functionality for translation
    
    manager.RegisterItem(new Item(ItemId.Homing, "Find Your Way", "Homing projectiles", Item.Tier.Rare)
      .AddSpriteFull(GD.Load<Texture2D>(Path.Combine(AssetPaths.AssetPath, AssetPaths.HomingPath)))
      .AddSpriteInventory()
      .AddModifier(ProjectileModifiers.Modifier.Homing),
      ItemLootPool.Treasure);
  }
}
