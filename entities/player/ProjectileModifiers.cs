using Flamme.items;
using System.Collections.Generic;
using System.Linq;

namespace Flamme.entities.player;

public class ProjectileModifiers
{
  public bool IsHoming { get; private set; } = false;

  public enum Modifier
  {
    Homing
  }

  public void AddModifier(Modifier modifier)
  {
    switch (modifier)
    {
      case Modifier.Homing:
        IsHoming = true;
        break;
    }
  }

  public void ClearModifiers()
  {
    IsHoming = false;
  }

  public void Update(List<Item> heldItems)
  {
    foreach (var modifier in heldItems.SelectMany(item => item.ProjectileModifiers))
    {
      AddModifier(modifier);
    }
  }
}
