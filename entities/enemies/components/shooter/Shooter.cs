using Godot;
using System;
using Flamme.testing;
using Flamme.world;
using Flamme.world.rooms;

public partial class Shooter : Node2D
{ 
  [Export] public PackedScene ProjectileScene;
  
  // This can all be changed on the fly
  [Export] public float Speed = 5.0f;
  [Export] public float Cooldown = 1.0f;
  [Export] public float Range = 128.0f;
  [Export] public int Damage = 1;

  [ExportGroup("Spawn")] 
  [Export] public float SpawnOffsetInDirection = 8.0f;
  
  [ExportGroup("Multishot")]
  [Export] public int ProjectileCount = 1;
  // Describes the MAX + and - degree that the bullet will have from the target
  [Export] public float SpreadDegree = 15.0f;
  [Export] public float SpawnSpreadPosV = 0.0f; 
  
  [ExportGroup("Flags")]
  [Export] public bool Homing = false;

  private Enemy _shooter;
  private PlayableCharacter _target;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public void Shoot(Enemy enemy, PlayableCharacter target)
  {
    _shooter = enemy;
    _target = target;

    // TODO 1 Perf split multishot vs non multishot
    Shoot();
  }

  private void Shoot()
  {
    var directionVec = _target.GlobalPosition - _shooter.GlobalPosition;

    // Don't shoot if out of range
    if (directionVec.Length() > Range)
    {
      return;
    }
    
    var offset = ProjectileCount % 2 == 1 ? 0.0f : SpreadDegree / 2;
    
    var directDirection = directionVec.Normalized();
    if (ProjectileCount % 2 == 1)
    {
      // uneven number, send one in the middle 
      GenShot(_shooter.GlobalPosition + (directDirection * SpawnOffsetInDirection), directDirection);
    }

    if (ProjectileCount == 1)
      return;
    
    var degreeStep = SpreadDegree / ProjectileCount;
    
    var posOffsetDirection = directionVec.Rotated(float.DegreesToRadians(90)).Normalized();
    var positionStep = SpawnSpreadPosV / ProjectileCount;
    
    for (var i = 0; i < ProjectileCount / 2; i++)
    {
      var multiplier = i + 1;
      
      // Send one with + and one with - degrees
      // Also includes +- offset in the global position
      var spawnPos = _shooter.GlobalPosition + (directDirection * SpawnOffsetInDirection)
                     + (positionStep * multiplier * -posOffsetDirection);
      directionVec = _target.GlobalPosition - spawnPos;
      var direction = directionVec.Rotated(float.DegreesToRadians(degreeStep * multiplier)).Normalized();
      GenShot(spawnPos, direction);
      
      var spawnPos2 = _shooter.GlobalPosition + (directDirection * SpawnOffsetInDirection)
                     + (positionStep * multiplier * posOffsetDirection);
      directionVec = _target.GlobalPosition - spawnPos2;
      var direction2 = directionVec.Rotated(float.DegreesToRadians(-degreeStep * multiplier)).Normalized();
      GenShot(spawnPos2, direction2);
    }
  }

  private void GenShot(Vector2 spawnPos, Vector2 direction)
  {
    var projectile = ProjectileScene.Instantiate<EnemyProjectile>();
    projectile.GlobalPosition = spawnPos;
    projectile.Direction = direction;
    var targetRotation = projectile.Direction.Angle();
    projectile.Rotation = targetRotation;
    GetTree().Root.AddChild(projectile);
    projectile.Fire(_shooter, Room.Current, _target, Range);
  }
}
