using Flamme.world;
using Flamme.world.rooms;
using Godot;
using System;

public partial class FireflyHomer : EnemyProjectile
{
  private PlayableCharacter _target;
  
  public override void _Ready()
  {
    // TODO 1 Do in editor just buggy rn
    DissipateTime = 0.3f;
  }

  protected override void CustomFireExec(Enemy enemy, Room room)
  {
    // TODO 1 Change when multiple targets needed
    _target = LevelManager.Instance.CurrentLevel.PlayableCharacter;
  }
  
  public override void _PhysicsProcess(double delta)
  {
    var exactDirection = _target.GlobalPosition - GlobalPosition;
    Direction = Direction.Lerp(exactDirection, 0.001f).Normalized();
    base._PhysicsProcess(delta);
  }
}
