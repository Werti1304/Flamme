using Flamme.entities.env.Loot;
using Godot;

namespace Flamme.entities.env.lying_around_loot;

public partial class LyingAroundLoot : Node2D
{
  [Export(PropertyHint.Range, "1,100,0.999")]
  public float SpawnChance = 100.0f;
  
  public override void _Ready()
  {
    if (GD.Randf() <= SpawnChance / 100.0f)
    {
      var lootToSpawn = LootGenerator.Instance.GenerateLyingAroundLoot();
      LootGenerator.SpawnLootAt(lootToSpawn, GlobalPosition, false);
    }
  }
}