using Flamme.testing;
using Godot;
using System;

public partial class MovingStatue : AnimatableBody2D
{
  [ExportGroup("Meta")]
  [Export] public Area2D PlayerDetectionArea;
  [Export] public AnimationPlayer AnimationPlayer;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    PlayerDetectionArea.BodyEntered += PlayerDetectionAreaOnBodyEntered;
  }

  private void PlayerDetectionAreaOnBodyEntered(Node2D body)
  {
    if (body is not PlayableCharacter)
      return;
    
    PlayerDetectionArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
    PlayerDetectionArea.BodyEntered -= PlayerDetectionAreaOnBodyEntered;
    
    AnimationPlayer.Play("move_left");
  }
}
