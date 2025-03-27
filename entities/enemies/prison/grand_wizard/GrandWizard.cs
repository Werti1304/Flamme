using Flamme.world.rooms;
using Godot;
using System;

namespace Flamme.entities.enemies.prison.grand_wizard;

public partial class GrandWizard : Enemy
{
  [Export] public float Speed = 10.0f;
  [Export] public float AttackTimerSec = 3.0f;

  [Export] public float Range = 312.0f;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public components.shooter.Shooter ShooterRapid;
  [Export] public components.shooter.Shooter ShooterHoming;
  [Export] public components.shooter.Shooter ShooterSpiral;
  [Export] public components.shooter.Shooter ShooterNormal;
  [Export] public components.health_bar.HealthBar HealthBar;
  
  private double _attackTimer;

  public override void _Ready()
  {
    base._Ready();
    
    HealthChanged += HealthBar.OnHealthChanged;
    HealthChanged += OnHealthChanged;
    HealthBar.OnHealthChanged(this);
  }

  private void OnHealthChanged(Enemy enemy)
  {
    AttackTimerSec = (Health / MaxHealth) switch
    {
      <= 0.33f => 2.0f,
      <= 0.66f => 2.5f,
      _ => AttackTimerSec
    };
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    var direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
    if(GlobalPosition.DistanceTo(Target.GlobalPosition) < Range)
    {
      Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
      
      if (direction.X < 0 && Sprite.FlipH)
      {
        Sprite.FlipH = false;
      }
      else if (direction.X > 0 && !Sprite.FlipH)
      {
        Sprite.FlipH = true;
      }
      
      if (_currentAttack == Attack.ShootRapid && _attackTimer >= 0.7f)
      {
        _shootCounter++;

        if (_shootCounter >= 6)
        {
          _currentAttack = Attack.None;
        }
        if (_shootCounter % 2 == 0)
        {
          ShooterRapid.ProjectileCount = 15;
        }
        else
        {
          ShooterRapid.ProjectileCount = 16;
        }
        ShooterRapid.Shoot(this, _initialTargetPos);
        _attackTimer = 0.0f;
      }
      else if (_currentAttack == Attack.ShootSpiral && _attackTimer >= 0.1f)
      {
        _shootCounter++;

        var chanceToStop = Math.Pow(1 / 40.0f * _shootCounter, 2);

        if (Main.Instance.Rnd.Randf() > chanceToStop)
        {
          ShooterSpiral.Shoot(this, Target);
        }
        else
        {
          _currentAttack = Attack.None;
        }

        _attackTimer = 0.0f;
      }
      else if (_attackTimer >= AttackTimerSec)
      {
        AttackTimerOnTimeout();
        _attackTimer = 0.0f;
      }

      else
      {
        _attackTimer += delta;
      }
    }
    else
    {
      Velocity = Velocity.Lerp(direction * Speed, 0.05f);
      
      if (direction.X < 0 && Sprite.FlipH)
      {
        Sprite.FlipH = false;
      }
      else if (direction.X > 0 && !Sprite.FlipH)
      {
        Sprite.FlipH = true;
      }
    }
    
    MoveAndSlide();
  }

  private enum Attack
  {
    None,
    SpawnHomingFlies,
    ShootCircle,
    SpawnRagingFlies,
    ShootDirect,
    ShootRapid,
    ShootHoming,
    ShootSpiral
  }

  private Attack _currentAttack = Attack.None;
  private int _shootCounter = 0; // General shoot counter, used for different attacks
  private Vector2 _initialTargetPos;
  private void AttackTimerOnTimeout()
  {
    _shootCounter = 0;
    var rand = Main.Instance.Rnd.RandiRange(0, 100);

    _currentAttack = rand switch
    {
      < 20 when Room.Current.Enemies.Count <= 2 => Attack.SpawnHomingFlies,
      < 40 when Room.Current.Enemies.Count <= 2 => Attack.SpawnRagingFlies,
      < 50 => Attack.ShootDirect,
      < 70 => Attack.ShootCircle,
      < 80 => Attack.ShootRapid,
      < 90 => Attack.ShootHoming,
      _ => Attack.ShootSpiral
    };

    switch (_currentAttack)
    {
      case Attack.None:
        return;
      case Attack.SpawnHomingFlies:
        var spawnDirection = GlobalPosition.DirectionTo(Target.GlobalPosition);
        var perpendicular = new Vector2(-spawnDirection.Y, spawnDirection.X).Normalized();
        var mirroredPosition1 = GlobalPosition + perpendicular * 32;
        var mirroredPosition2 = GlobalPosition - perpendicular * 32;
        SpawnEnemy(Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.FireflyHoming], mirroredPosition1);
        SpawnEnemy(Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.FireflyHoming], mirroredPosition2);
        break;
      case Attack.SpawnRagingFlies:
        for (var i = 0; i < 5; i++)
        {
          SpawnEnemy(Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.FireflyRaging], GlobalPosition, 16);
        }
        break;
      case Attack.ShootRapid:
        // Random initial position
        _initialTargetPos = GlobalPosition + 
                            new Vector2(Main.Instance.Rnd.RandfRange(-32, 32), Main.Instance.Rnd.RandfRange(-32, 32));
        return; // Handled in _PhysicsProcess
      case Attack.ShootHoming:
        ShooterHoming.Shoot(this, Target);
        break;
      case Attack.ShootSpiral:
        return;
      case Attack.ShootCircle:
        ShooterRapid.ProjectileCount = 15;
        ShooterRapid.Shoot(this, Target);
        break;
      case Attack.ShootDirect:
        var count = Main.Instance.Rnd.RandiRange(1, 3);
        ShooterNormal.ProjectileCount = count;
        ShooterNormal.Shoot(this, Target);
        ShooterNormal.ProjectileCount = 1;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    _currentAttack = Attack.None;
  }
}