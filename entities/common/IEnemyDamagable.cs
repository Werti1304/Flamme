namespace Flamme.entities.common;

/// <summary>
/// Damagable by enemies
/// </summary>
public interface IEnemyDamagable
{
  public void TakeDamage(int damage);
}
