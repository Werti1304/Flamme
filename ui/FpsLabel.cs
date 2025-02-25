using Godot;
using System;
using System.Globalization;

public partial class FpsLabel : Label
{
  private double TIMER_LIMIT = 2.0;
  private double timer = 0.0;
  
  public override void _Process(double delta)
  {
    timer += delta;
    if (timer > TIMER_LIMIT)
    {
      Text = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
    }
  }
}
