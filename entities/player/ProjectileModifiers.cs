using Flamme.items;
using Flamme.spells;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flamme.entities.player;

public class ProjectileModifiers
{
  public bool IsHoming { get; private set; }
  public bool IsFireball { get; private set; }
  public bool IsBlargh { get; private set; }

  public enum Modifier
  {
    Homing,
    Fireball,
    Blargh
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
      case Modifier.Blargh:
        IsBlargh = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
    }
  }

  public void ClearModifiers()
  {
    IsHoming = false;
    IsFireball = false;
    IsBlargh = false;
  }

  public void Update(List<Item> heldItems, List<Spell> activeSpells)
  {
    ClearModifiers();
    
    foreach (var modifier in heldItems.SelectMany(item => item.ProjectileModifiers))
    {
      AddModifier(modifier);
    }
    
    foreach (var modifier in activeSpells.SelectMany(spell => spell.ProjectileModifiers))
    {
      AddModifier(modifier);
    }
  }
}
