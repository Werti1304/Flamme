using Flamme.items;
using Flamme.world.generation;
using Godot;

namespace Flamme;

public partial class Main : Node
{
  // TODO 2 Item Inventory
  // TODO 2 Item List ingame
  // TODO 3 Main Menu
  // TODO 4 Minimap (After world gen?)
  // TODO 2 Add Staff with physics (pulsing as long as not picked up)
  // TODO 2 Add Bullet, then add Trail, then into small streaks like ori:blind forest/fern offensive magic
  // TODONE Stats in HUD
  public override void _Ready()
  {
    // Use GD.[...] for seeded stuff -> level Layout, Room Layout, Items, Enemies, Chest Contents, etc.
    // GD.Seet() For when we want to use seed
    GD.Randomize();
    
    WorldGenerator.Instance.GenerateLevels();
  }

  public Main()
  {
    StatUpItems.RegisterStatUpItems();
  }

}
