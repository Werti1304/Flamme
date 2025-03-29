using Godot;

namespace Flamme.common.input;

public static class PlayerInputMap
{
  public enum Action
  {
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    ShootUp,
    ShootDown,
    ShootRight,
    ShootLeft,
    Pause,
    ToggleMap,
    Interact,
    Interact2
  }

  public static Action? GetPressedAction(InputEvent @event)
  {
    foreach (var actionEntry in Dict)
    {
      if (@event.IsActionPressed(actionEntry.Value))
      {
        return actionEntry.Key;
      }
    }
    return null;
  }
  
  public static readonly Godot.Collections.Dictionary<Action, string> Dict = new Godot.Collections.Dictionary<Action, string>()
  {
    { Action.MoveUp, "move_up" },
    { Action.MoveDown, "move_down" },
    { Action.MoveLeft, "move_left" },
    { Action.MoveRight, "move_right" },
    { Action.ShootUp, "shoot_up" },
    { Action.ShootDown, "shoot_down" },
    { Action.ShootRight, "shoot_right" },
    { Action.ShootLeft, "shoot_left" },
    { Action.Pause, "open_escape_menu" },
    { Action.ToggleMap, "toggle_map" },
    { Action.Interact, "interact" },
    { Action.Interact2, "interact_space" }
  };
}