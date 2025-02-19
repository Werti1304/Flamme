using Godot;
using System;

public static class Const
{
  public static class InputMap
  {
    public const string MoveUp = "move_up";
    public const string MoveDown = "move_down";
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";

    public static readonly string[] Move = new[] { MoveUp, MoveDown, MoveLeft, MoveRight };
  }
}
