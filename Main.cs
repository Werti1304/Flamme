using Flamme.entities.env.Loot;
using Flamme.items;
using Flamme.world;
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
  // TODO 2 make it so items are removed from loot pool when generating loot for the level and re-add everything the player...
  // TODO ...didn't pick up after leaving the level
  
  
  // TODO OK we need to fix a few things
  // 1. Set Room Cleared MUST only happen while player inside and enemies not existing
  // or all gone
  // 2. Make Loot generation in a streamlined process (have fun lol)
  // 3. Make actual loot spawning correct
  // 4. Make loot spawn in level generation
  public override void _Ready()
  {
    // Use GD.[...] for seeded stuff -> level Layout, Room Layout, Items, Enemies, Chest Contents, etc.
    // GD.Seet() For when we want to use seed
    //GD.Randomize();
    GD.Seed(1234);
  }

  public override void _Notification(int what)
  {
    if (what == NotificationWMCloseRequest)
    {
      GetTree().CurrentScene.ProcessMode = ProcessModeEnum.Disabled;
      GetTree().Quit();
    }
  }

  public Main()
  {
    StatUpItems.RegisterStatUpItems();
    
    DefaultLoot.RegisterDefaultLoot();
  }
}
