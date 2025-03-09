using Flamme.items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flamme.entities.player;

public class ProjectileModifiers
{
  public bool IsHoming { get; private set; } = false;
  public bool IsFireball { get; private set; } = false;

  public enum Modifier
  {
    Homing,
    Fireball
  }

  public void AddModifier(Modifier modifier)
  {
    switch (modifier)
    {
      case Modifier.Homing:
        IsHoming = true;
        break;
      case Modifier.Fireball:
        IsFireball = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
    }
  }

  public void ClearModifiers()
  {
    IsHoming = false;
    IsFireball = false;
  }

  public void Update(List<Item> heldItems)
  {
    foreach (var modifier in heldItems.SelectMany(item => item.ProjectileModifiers))
    {
      AddModifier(modifier);
    }
  }
}
