using Flamme.common.input;
using Godot;

namespace Flamme.ui.key_press;

public partial class KeyPress : Sprite2D
{
  [Export] public Color ModulateColor = Colors.ForestGreen;
  [Export] public Color ModulateCurrentPressedColor = Colors.Lime;
  [Export] public PlayerInputMap.Action ActionToTriggerModulate = PlayerInputMap.Action.ShootUp;

  [Export] public Texture2D ControllerTexture;
  private Texture2D _defaultTexture;
  
  public bool WasPressed;

  public override void _Ready()
  {
    // Doesn't need to be pressed in case of pause, because that would just be unneccessarily tedious every time
    if (ActionToTriggerModulate == PlayerInputMap.Action.Pause)
    {
      Modulate = ModulateColor;
      WasPressed = true;
    }
    _defaultTexture = Texture;
    
    Main.Instance.PlayerInputDeviceChanged += OnPlayerInputDeviceChanged;
  }

  protected override void Dispose(bool disposing)
  {
    Main.Instance.PlayerInputDeviceChanged -= OnPlayerInputDeviceChanged;
    
    base.Dispose(disposing);
  }

  private void OnPlayerInputDeviceChanged()
  {
    if (ControllerTexture == null)
      return;
    
    if (Main.Instance.PlayerUsingController)
    {
      Texture = ControllerTexture;
    }
    else
    {
      Texture = _defaultTexture;
    }
  }

  private bool _wasPressed;
  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouse)
      return;

    if (Input.IsActionPressed(PlayerInputMap.Dict[ActionToTriggerModulate]) && Input.GetActionStrength(PlayerInputMap.Dict[ActionToTriggerModulate]) > 0.8f)
    {
      Modulate = ModulateCurrentPressedColor;
      _wasPressed = true;
    }
    else if (@event.IsActionReleased(PlayerInputMap.Dict[ActionToTriggerModulate]) && _wasPressed)
    {
      Modulate = ModulateColor;
      WasPressed = true;
    }
  }
}