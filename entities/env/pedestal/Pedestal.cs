using Flamme.entities.env;
using Godot;

public partial class Pedestal : StaticBody2D
{
  [Export] public ItemPickup ItemPickupLoot;

  public override void _Ready()
  {
    if (ItemPickupLoot == null)
    {
      foreach (var child in GetChildren())
      {
        if (child is ItemPickup itemPickup)
        {
          ItemPickupLoot = itemPickup;
          break;
        }
      }
    }

    if (ItemPickupLoot == null)
    {
      GD.PrintErr($"No ItemPickup found in Pedestal {Name} at coords {GlobalPosition}");
    }
    else
    {
      ItemPickupLoot.ShowItem();
    }
  }
}
