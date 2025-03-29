using Flamme.common.enums;
using Flamme.entities.player;
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
      .SetUptime(3.0f)
      .SetCooldownRooms(3));
  }
}
