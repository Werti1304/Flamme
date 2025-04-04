using Flamme.entities.env.Loot;
using Flamme.items;
using Flamme.spells;
using Flamme.world;
using Flamme.world.generation;
using Godot;

namespace Flamme;

public partial class Main : Node
{
  // Non-Seeded Random Generator!
  public RandomNumberGenerator Rnd = new RandomNumberGenerator();
  
  // TODO 2 Item Inventory
  // TODO 2 Item List ingame
  // TODO 3 Main Menu
  // TODO 2 Add Staff with physics (pulsing as long as not picked up)
  // TODO 2 make it so items are removed from loot pool when generating loot for the level and re-add everything the player...
  // TODO ...didn't pick up after leaving the level
  
  // TODO 3 Chest loot generation per LootPool
  
  // TODO Cleaning up
  // TODO Clean rooms up
  // TODO Clean testing stuff up
  // TODO Clean general up to new standards
  
  // TODO OK we need to fix a few things
  // 1. Set Room Cleared MUST only happen while player inside and enemies not existing
  // or all gone
  // 2. Make Loot generation in a streamlined process (have fun lol)
  // 3. Make actual loot spawning correct
  // 4. Make loot spawn in level generation
  
  // Small TODO s
  // Make staff trail speed scale better with speed, looks weird rn
  // Make staff come with u even while youre holding it when changing rooms
  // Fix enemy spawning in wall thats not good
  // Make it so boss room 1 is on level 1 and 2 on 2

  [Signal] public delegate void PlayerInputDeviceChangedEventHandler();
  public bool PlayerUsingController { get; private set; }

  public override void _Input(InputEvent @event)
  {
    if (!PlayerUsingController && @event is InputEventJoypadButton || @event is InputEventJoypadMotion)
    {
      PlayerUsingController = true;
      EmitSignal(SignalName.PlayerInputDeviceChanged);
    }
    else if(PlayerUsingController && @event is InputEventKey)
    {
      PlayerUsingController = false;
      EmitSignal(SignalName.PlayerInputDeviceChanged);
    }
  }

  public bool UnloadingLevel;

  public override void _Notification(int what)
  {
    if (what == NotificationWMCloseRequest || what == NotificationPredelete)
    {
      Shutdown();
    }
  }

  public void Shutdown()
  {
    if (UnloadingLevel)
      return;
    GD.Print($"SHUTDOWN REQUESTED");
    UnloadingLevel = true;
    GetTree().Quit();
  }

  public Main()
  {
    _instance = this;
    
    GD.Print("Registering Stat-Up Items...");
    StatUpItems.Register();
    GD.Print("Done!\nRegistering Modifier Items");
    ModifierItems.RegisterItems();
    GD.Print("Done!\nRegistering Spells");
    StatUpSpells.Register();
    GD.Print("Done!\nRegistering Default Loot");
    DefaultLoot.RegisterDefaultLoot();
    GD.Print("Done!");
  }

  public void StartNewGame(ulong seed)
  {
    ResetPersistantData();
    WorldGenerator.Instance.GenerateLevels(seed);
  }

  private void ResetPersistantData()
  {
    LevelManager.Instance.Reset();
  }

  public static Main Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }

  private static Main _instance;
  private static readonly object Padlock = new object();
}
