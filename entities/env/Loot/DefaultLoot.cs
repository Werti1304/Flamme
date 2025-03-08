#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // DefaultLoot.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Flamme.common.enums;
using System.Collections.Generic;

namespace Flamme.entities.env.Loot;

public static class DefaultLoot
{
  public static void RegisterDefaultLoot()
  {
    RegisterPathwayLoot();
    RegisterLockedChestLoot();
  }

  private static void RegisterLockedChestLoot()
  {
    const LootPool lootPool = LootPool.LockedChest;
    // Chances should add up to 100, as it's already decided that it's gonna be one of these
    var list = new List<LootGenerator.LootMeta>()
    {
      new LootGenerator.LootMeta(LootGenerator.LootType.NormalHealth, 10, 1, 2, 2),
      new LootGenerator.LootMeta(LootGenerator.LootType.NormalHealth, 15, 1, 4, 4),
      new LootGenerator.LootMeta(LootGenerator.LootType.Coin, 25, 1, 1, 10),
      new LootGenerator.LootMeta(LootGenerator.LootType.AbsorptionHealth, 25, 1, 4, 4),
      new LootGenerator.LootMeta(LootGenerator.LootType.Key, 25, 1, 1, 1),
    };
    
    LootGenerator.Instance.RegisterLoot(lootPool, list);
  }

  private static void RegisterPathwayLoot()
  {
    const LootPool lootPool = LootPool.Pathway;
    // Chances should add up to 100, as it's already decided that it's gonna be one of these
    var list = new List<LootGenerator.LootMeta>()
    {
      // new LootGenerator.LootMeta(LootGenerator.LootType.NormalHealth, 20, 1, 2, 2),
      // new LootGenerator.LootMeta(LootGenerator.LootType.NormalHealth, 10, 1, 4, 4),
      // new LootGenerator.LootMeta(LootGenerator.LootType.Coin, 25, 1, 1, 5),
      // new LootGenerator.LootMeta(LootGenerator.LootType.AbsorptionHealth, 10, 1, 1, 4),
      // new LootGenerator.LootMeta(LootGenerator.LootType.Key, 20, 1, 1, 1),
      // new LootGenerator.LootMeta(LootGenerator.LootType.NormalChest, 10, 1, 1, 1),
      // new LootGenerator.LootMeta(LootGenerator.LootType.LockedChest, 5, 1, 1, 1),
      // new LootGenerator.LootMeta(LootGenerator.LootType.MimicChest, 5, 1, 1, 1),
      new LootGenerator.LootMeta(LootGenerator.LootType.NormalChest, 100, 1, 1, 1),
      new LootGenerator.LootMeta(LootGenerator.LootType.LockedChest, 0, 1, 1, 1),
      new LootGenerator.LootMeta(LootGenerator.LootType.MimicChest, 0, 1, 1, 1),
    };
    
    LootGenerator.Instance.RegisterLoot(lootPool, list);
  }
}
