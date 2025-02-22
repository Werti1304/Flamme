using Godot;
using System;

public interface IPlayerDamageable
{
  public float GetHealth();
  public void Damage(float attackDamage, float knockBackStrength, Vector2 attackDirection);
}
