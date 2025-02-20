using Godot;
using System;
using System.Collections.Generic;

public static class Const
{
  public static class InputMap
  {
    [Flags]
    public enum Action
    {
      None =      0,
      MoveUp =    0x1,
      MoveDown =  0x2,
      MoveLeft =  0x4,
      MoveRight = 0x8,
      ShootUp =   0x10,
      ShootDown = 0x20,
      ShootRight =0x40,
      ShootLeft = 0x80
    }

    public static readonly Godot.Collections.Dictionary<Action, string> ActionInputDict = new Godot.Collections.Dictionary<Action, string>()
    {
      { Action.MoveUp, "move_up" },
      { Action.MoveDown, "move_down" },
      { Action.MoveLeft, "move_left" },
      { Action.MoveRight, "move_right" },
      { Action.ShootUp, "shoot_up" },
      { Action.ShootDown, "shoot_down" },
      { Action.ShootRight, "shoot_right" },
      { Action.ShootLeft, "shoot_left" }
    };
  }
  
  [Flags]
  public enum Facing
  {
    Up =    0x1,
    Down =  0x2,
    Left =  0x4,
    Right = 0x8
  }
  
  public static readonly Dictionary<Facing, Vector2> FacingNormVecDict = new Dictionary<Facing, Vector2>()
  {
    { Facing.Up, Vector2.Up },
    { Facing.Down, Vector2.Down },
    { Facing.Left, Vector2.Left },
    { Facing.Right, Vector2.Right },
  };
}
