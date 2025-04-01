using Flamme.entities.enemies.components.melee_area;
using Godot;
using System;

namespace Flamme.entities.enemies.prison.slider;

public partial class Slider : Enemy
{
  [Export] public int RunnersSpawnedOnDeath = 7;
  [Export] public float WaitTimeRandomnessPercentage = 0.3f;
  [Export] public float KnockbackMultiplier = 2.0f;
  
  [ExportGroup("Phase Specific")]
  [Export] public float SpeedPhase1 = 150.0f;
  [Export] public float SpeedPhase2 = 225.0f;
  [Export] public float SpeedPhase3 = 350.0f;
  [Export] public float WaitTimePhase1 = 3.0f;
  [Export] public float WaitTimePhase2 = 2f;
  [Export] public float WaitTimePhase3 = 1f;
  
  [ExportGroup("Textures")]
  [Export] public Texture2D TexturePhase1;
  [Export] public Texture2D TexturePhase2;
  [Export] public Texture2D TexturePhase3;
  
  [ExportGroup("Meta")]
  [Export] public components.health_bar.HealthBar HealthBar;
  [Export] public Sprite2D Body;
  [Export] public Sprite2D Eye;
  [Export] public Node2D EyeCenter;
  [Export] public Timer ChangeDirectionTimer;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public MeleeArea MeleeArea;
  [Export] public GpuParticles2D DeathParticles;

  private static readonly Vector2I EyeSize = new Vector2I(10, 5);
  
  private Vector2 _direction;
  private double _directionTimerDefault;
  
  private float _speed;

  private enum Phase
  {
    Phase1,
    Phase2,
    Phase3,
  }
  
  private Phase _currentPhase = Phase.Phase1;
  private Phase CurrentPhase
  {
    get => _currentPhase;
    set
    {
      _currentPhase = value;

      switch (value)
      {
        case Phase.Phase1:
          Body.Texture = TexturePhase1;
          ChangeDirectionTimer.WaitTime = WaitTimePhase1;
          _speed = SpeedPhase1;
          break;
        case Phase.Phase2:
          ChangeDirectionTimer.WaitTime = WaitTimePhase2;
          Body.Texture = TexturePhase2;
          _speed = SpeedPhase2;
          break;
        case Phase.Phase3:
          ChangeDirectionTimer.WaitTime = WaitTimePhase3;
          Body.Texture = TexturePhase3;
          _speed = SpeedPhase3;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(value), value, null);
      }
    }
  }
  
  public override void _Ready()
  {
    base._Ready();
    
    _directionTimerDefault = ChangeDirectionTimer.WaitTime;
    
    HealthChanged += HealthBar.OnHealthChanged;
    HealthChanged += OnHealthChanged;
    
    HealthBar.OnHealthChanged(this);
    
    ChangeDirectionTimer.Timeout += ChangeDirectionTimerOnTimeout;
    
    MeleeArea.BodyEntered += MeleeAreaOnBodyEntered;
    
    CurrentPhase = Phase.Phase1;
  }

  private void MeleeAreaOnBodyEntered(Node2D body)
  {
    if (body is player.PlayableCharacter playableCharacter)
    {
      playableCharacter.Velocity += GlobalPosition.DirectionTo(body.GlobalPosition) * Mathf.Max(200.0f, Velocity.Length() * KnockbackMultiplier);
    }
  }

  protected override void OnSetActive()
  {
    ChangeDirectionTimer.Start();
  }

  protected override void OnSetPassive()
  {
    ChangeDirectionTimer.Stop();
  }

  protected override void PhysicsProcess(double delta)
  {
    if (!IsActive)
      return;
    
    var lookingDirection = (Target.GlobalPosition - EyeCenter.GlobalPosition).Normalized();
    var pupilPosition = (Vector2I)(EyeCenter.Position + new Vector2I((int)(lookingDirection.X * EyeSize.X), (int)(lookingDirection.Y * EyeSize.Y)));
    Eye.Position = pupilPosition;
    
    Velocity = _direction * _speed;
  }

  public override void OnDeath()
  {
    var runnerScene = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.Runner];
    var smartRunnerScene = Flamme.common.scenes.SceneLoader.Instance[Flamme.common.scenes.SceneLoader.Scene.RunnerSmart];
    
    SetPhysicsProcess(false);
    Body.Hide();
    Eye.Hide();
    DeathParticles.Emitting = true;

    for (var i = 0; i < RunnersSpawnedOnDeath; i++)
    {
      var randomPosOffset = new Vector2(Main.Instance.Rnd.RandfRange(-8, 8), Main.Instance.Rnd.RandfRange(-8, 8));
      if (Main.Instance.Rnd.Randf() < 0.3f)
      {
        SpawnEnemy(smartRunnerScene, GlobalPosition + randomPosOffset);
      }
      else
      {
        SpawnEnemy(runnerScene, GlobalPosition + randomPosOffset);
      }
    }
    DeathParticles.Finished += base.OnDeath;
  }

  private void ChangeDirectionTimerOnTimeout()
  {
    if (Target == null)
      return;
    
    var directionTo = GlobalPosition.DirectionTo(Target.GlobalPosition);
    var directionToAbs = directionTo.Abs();
    
    // Target is around diagonal to us, so theres a chance we go diagonal to him
    if (directionToAbs.X - directionToAbs.Y < 0.5f && Main.Instance.Rnd.Randf() < 0.1f)
    {
      _direction = directionTo;
    }
    else if (directionToAbs.X > directionToAbs.Y)
    {
      _direction = new Vector2(directionTo.X, 0);
    }
    else
    {
      _direction = new Vector2(0, directionTo.Y);
    }
    ChangeDirectionTimer.WaitTime = _directionTimerDefault + Main.Instance.Rnd.RandfRange(-WaitTimeRandomnessPercentage, WaitTimeRandomnessPercentage) * _directionTimerDefault;
  }
  
  private void OnHealthChanged(Enemy enemy)
  {
    var percentage = Health / MaxHealth;

    CurrentPhase = percentage switch
    {
      < 0.33f => Phase.Phase3,
      < 0.66f => Phase.Phase2,
      _ => Phase.Phase1
    };
  }
}