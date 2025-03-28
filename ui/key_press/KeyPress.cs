using Flamme.common.input;
using Godot;

namespace Flamme.ui.key_press;

public partial class KeyPress : Sprite2D
{
  [Export] public Color ModulateColor = Colors.ForestGreen;
  [Export] public Color ModulateCurrentPressedColor = Colors.Lime;
  [Export] public PlayerInputMap.Action ActionToTriggerModulate = PlayerInputMap.Action.ShootUp;
  
  public bool WasPressed;

  public override void _Ready()
  {
    // Doesn't need to be pressed in case of pause, because that would just be unneccessarily tedious every time
    if (ActionToTriggerModulate == PlayerInputMap.Action.Pause)
    {
      Modulate = ModulateColor;
      WasPressed = true;
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouse)
      return;

    if (Input.IsActionPressed(PlayerInputMap.Dict[ActionToTriggerModulate]))
    {
      Modulate = ModulateCurrentPressedColor;
    }
    else if (@event.IsActionReleased(PlayerInputMap.Dict[ActionToTriggerModulate]))
    {
      Modulate = ModulateColor;
      WasPressed = true;
    }
  }
}