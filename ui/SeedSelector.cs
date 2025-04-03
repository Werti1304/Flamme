using Godot;

namespace Flamme.ui;

public partial class SeedSelector : SpinBox
{
  private bool _stepable = true;

  public override void _Ready()
  {
    GetViewport().GuiFocusChanged += OnGuiFocusChanged;
    SetProcessInput(false);
    SetPhysicsProcess(false);
  }

  private void OnGuiFocusChanged(Control node)
  {
    // Somehow node == doesn't work
    if (node is LineEdit)
    {
      GD.Print("focus");
      SetProcessInput(true);
      SetPhysicsProcess(true);
      holdTime = 0f;
      _firstPress = true;
      _totalHoldTime = 0.0f;
    }
    else
    {
      SetProcessInput(false);
      SetPhysicsProcess(false);
    }
  }

  private float _totalHoldTime = 0.0f; 
  
  private bool _firstPress = true;
  private float holdTime = 0f;
  private const float repeatDelay = 0.5f; // Initial delay before repeating
  private float repeatRate = 0.1f; // Time between repeats
  public override void _PhysicsProcess(double delta)
  {
    if (Input.IsActionPressed("ui_up"))
    {
      if (_firstPress)
      {
        _firstPress = false;
        holdTime = 0f;
        Value += Step;
        return;
      }
      holdTime += (float)delta;
      _totalHoldTime += (float)delta;
      
      if (_totalHoldTime >= 10.0f)
      {
        repeatRate = 0.001f;
        Step = 10;
      }
      else if (_totalHoldTime >= 6.0f)
      {
        repeatRate = 0.01f;
      }
      else if (_totalHoldTime >= 3.0f)
      {
        repeatRate = 0.05f;
      }
      else
      {
        repeatRate = 0.1f;
        Step = 1;
      }
      if (holdTime >= repeatDelay)
      {
        Value += Step;
        holdTime = repeatDelay - repeatRate; // Reduce delay for continuous change
      }
    }
    else if (Input.IsActionPressed("ui_down"))
    {
      if (_firstPress)
      {
        _firstPress = false;
        holdTime = 0f;
        Value -= Step;
        return;
      }
      holdTime += (float)delta;
      _totalHoldTime += (float)delta;
      if (holdTime >= repeatDelay)
      {
        Value -= Step;
        holdTime = repeatDelay - repeatRate;
      }
    }
    else
    {
      _firstPress = true;
      holdTime = 0f;
      _totalHoldTime = 0;
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event.IsAction("ui_up"))
    {
      GetViewport().SetInputAsHandled();
    }
    if (@event.IsAction("ui_down"))
    {
      GetViewport().SetInputAsHandled();
    }
  }
}