using Flamme.common.helpers;
using Godot;
using Room = Flamme.world.rooms.Room;

namespace Flamme.entities.enemies;

public abstract partial class Enemy : CharacterBody2D, IPlayerDamageable
{
  private float _health = 10;
  [Export] public bool EnemyDisabled = false;
  [Export] public float Health
  {
    get => _health;
    set
    {
      _health = value;
      EmitSignal(SignalName.HealthChanged, this);
    } 
  }
  [Export] public float Weight = 10.0f;
  
  [Signal] public delegate void HealthChangedEventHandler(Enemy enemy);

  public float MaxHealth;
  
  protected bool IsActive => Target != null;

  protected player.PlayableCharacter Target;
  private Room _room;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    SetPhysicsProcess(false);
    
    MaxHealth = Health;
  }

  public virtual void OnDeath()
  {
    QueueFree(); 
  }
  
  /// <summary>
  /// When the enemy takes damage
  /// </summary>
  /// <param name="attackDamage">How much damage</param>
  /// <returns>Whether the enemy died (true)</returns>
  public bool TakeDamage(float attackDamage)
  {
    Health -= attackDamage;

    if (Health > 0)
    {
      return false;
    }
    Health = 0;
    OnDeath();
    return true;
  }

  public virtual void SetActive(player.PlayableCharacter playableCharacter)
  {
    if (playableCharacter == null)
    {
      GD.PushWarning("Tried to set enemy active with null playable character.");
      return;
    }
    if (EnemyDisabled)
    {
      return;
    }
    GD.Print($"Enemy {Name} active!");
    SetPhysicsProcess(true);
    Target = playableCharacter;
    OnSetActive();
  }
  
  protected virtual void OnSetActive()
  { }
  
  public virtual void SetPassive()
  {
    GD.Print($"Enemy {Name} passive again.");
    SetPhysicsProcess(false);
    OnSetPassive();
    Target = null;
  }
  
  protected virtual void OnSetPassive()
  { }

  public float GetHealth()
  {
    return Health;
  }


  protected virtual void OnHit()
  { }

  public virtual void Hit(float attackDamage, float knockBackStrength, Vector2 attackDirection)
  {
    OnHit();
    TakeDamage(attackDamage);
    Velocity += (knockBackStrength / Weight) * attackDirection;
  }
  
  protected static void SpawnEnemy(PackedScene toSpawn, Vector2 globalPosition, int randomizedPosPx = 0)
  {
    if (randomizedPosPx > 0)
    {
      globalPosition += new Vector2(Main.Instance.Rnd.RandiRange(-randomizedPosPx, randomizedPosPx), 
        Main.Instance.Rnd.RandiRange(-randomizedPosPx, randomizedPosPx));
    }
    var enemy = toSpawn.Instantiate();
    // TODO 2 prevent from spawning in the wall
    Room.Current.CallDeferred(Node.MethodName.AddChild, enemy);
    enemy.SetDeferred(Node2D.PropertyName.GlobalPosition, globalPosition);
  }
}