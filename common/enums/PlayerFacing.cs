using Godot;

namespace Flamme.common.enums;

public enum PlayerFacing
{
  Up,
  Down,
  Left,
  Right
}

public static class PlayerFacingMethods
{
  private const float FacingThreshold = 0.3f;
  public static PlayerFacing GetFacing(Vector2 shootingVector, Vector2 movingVector)
  {
    switch (shootingVector.Y)
    {
      case > FacingThreshold:
        return PlayerFacing.Down;
      case < -FacingThreshold:
        return PlayerFacing.Up;
    }

    switch (shootingVector.X)
    {
      case > FacingThreshold:
        return PlayerFacing.Right;
      case < -FacingThreshold:
        return PlayerFacing.Left;
    }
    
    switch (movingVector.Y)
    {
      case > FacingThreshold:
        return PlayerFacing.Down;
      case < -FacingThreshold:
        return PlayerFacing.Up;
    }
    
    switch (movingVector.X)
    {
      case > FacingThreshold:
        return PlayerFacing.Right;
      case < -FacingThreshold:
        return PlayerFacing.Left;
    }

    return PlayerFacing.Down;
  }
}