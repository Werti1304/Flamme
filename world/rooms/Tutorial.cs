using Flamme.world.generation;
using Godot;
using Godot.Collections;
using System.Linq;

namespace Flamme.world.rooms;

public partial class Tutorial : Node2D
{
  [Export] public Room TutorialRoom;
  [Export] public Array<ui.key_press.KeyPress> KeyPresses = [];

  public override void _Ready()
  {
    LevelManager.Instance.LevelReady += OnLevelReady;
  }

  private void OnLevelReady()
  {
    TutorialRoom.OverrideDoorLogic = true;
    foreach (var door in TutorialRoom.Doors.Values)
    {
      door.Lock();
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouse)
      return;

    var allPressed = KeyPresses.All(keyPress => keyPress.WasPressed);
    if (!allPressed)
      return;

    if (Level.Current.ActiveStaff == null)
      return;

    foreach (var door in TutorialRoom.Doors.Values)
    {
      door.OpenByClearingRoom();
    }
    TutorialRoom.OverrideDoorLogic = false;
    
    // Disable processing for all
    foreach (var keyPress in KeyPresses)
    {
      keyPress.SetProcessInput(false);
    }
    SetProcessInput(false);
  }
}