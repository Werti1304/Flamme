using Flamme.common.enums;
using Flamme.entities.player;
using Flamme.world.rooms;
using Godot;
using System;
using static Flamme.common.input.PlayerInputMap.Action;

namespace Flamme.spells;

public class StatUpSpells
{
  public static void Register()
  {
    var manager = SpellManager.Instance;
    
    manager.RegisterSpell(new Spell(SpellId.RapidFire, "Zoltraak", "Rapid Fire", 
      ShootUp, ShootDown, ShootLeft, ShootRight)
      .AddStatUp(StatType.FireMultiplier, 5)
      .AddModifier(ProjectileModifiers.Modifier.Homing)
      .SetUptime(3.0f).SetUptimeStartUponShooting()
      .SetCooldownRooms(3));

    var spell = new Spell(SpellId.DoorOpen, "Sesame", "Open All Doors",
        ShootLeft, ShootDown, ShootDown, ShootDown, ShootRight)
      .SetUptime(2.5f)
      .SetCooldownRooms(1)
      .SetChargeCrystalCost(2);
    spell.SpellCast += SpellOnSpellCast;
    manager.RegisterSpell(spell);
    
    manager.RegisterSpell(new Spell(SpellId.Blargh, "Blargh", "Blaaargh", 
        ShootUp, ShootDown, ShootUp, ShootDown, ShootUp, ShootUp)
      .AddStatUp(StatType.FireMultiplier, 0.1f)
      .AddStatUp(StatType.Damage, 10)
      .AddStatUp(StatType.DamageMultiplier, 3)
      .AddModifier(ProjectileModifiers.Modifier.Blargh)
      .SetUptime(5.0f).SetUptimeStartUponShooting()
      .SetCooldownRooms(5));
  }

  private static void SpellOnSpellCast(object sender, EventArgs e)
  {
    if (Room.Current != null)
    {
      Room.Current.OverrideDoorLogic = true;
      var tween = Room.Current.GetTree().CreateTween();
      foreach (var door in Room.Current.Doors)
      {
        tween.TweenInterval(0.5f);
        tween.TweenCallback(Callable.From(door.Value.ForceOpen));
      }
      Room.Current.OverrideDoorLogic = false;
    }
  }
}
