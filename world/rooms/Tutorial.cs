using Flamme.ui.key_press;
using Flamme.world.generation;
using Godot;
using Godot.Collections;
using System.Linq;

namespace Flamme.world.rooms;

public partial class Tutorial : Node2D
{
  [Export] public Room TutorialRoom;
  [Export] public Array<KeyPress> KeyPresses = new Array<KeyPress>();

  public static bool Completed;

  public static void Reset()
  {
    Completed = false;
  }
  
  public override void _Ready()
  {
    LevelManager.Instance.LevelReady += OnLevelReady;
  }

  private void OnLevelReady()
  {
    // Bc we kinda get called from LevelManager, even if this object is already kinda disposed lol
    if (!IsInstanceValid(this))
    {
      return;
    }
    // This is so that the tutorial won't be shown at the start of the next level
    if (Completed)
    {
      foreach (var keyPress in KeyPresses)
      {
        keyPress.SetProcessInput(false);
        keyPress.Visible = false;
      }
      SetProcessInput(false);
      return;
    }
    
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
      keyPress.Modulate = keyPress.ModulateColor;
    }
    Completed = true;
    SetProcessInput(false);
  }
}