using Godot;
using System;
using System.Collections.Generic;

public static class Const
{
  public static class InputMap
  {
    public enum Action
    {
      None,
      MoveUp,
      MoveDown,
      MoveLeft,
      MoveRight,
      ShootUp,
      ShootDown,
      ShootRight,
      ShootLeft
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
}
