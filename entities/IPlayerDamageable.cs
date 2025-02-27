using Godot;

namespace Flamme.entities;

public interface IPlayerDamageable
{
  public float GetHealth();
  public void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection);
}