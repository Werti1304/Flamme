using Godot;
using System;
using Flamme.common.enums;

public partial class PlayerSprite : Sprite2D
{
  [Export] public Texture2D Up;
  [Export] public Texture2D Down;
  [Export] public Texture2D Left;
  [Export] public Texture2D Right;

  public void OnFacingChange(PlayerFacing facing)
  {
    Texture = facing switch
    {
      PlayerFacing.Up => Up,
      PlayerFacing.Down => Down,
      PlayerFacing.Left => Left,
      PlayerFacing.Right => Right,
      _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null)
    };
  }
}
