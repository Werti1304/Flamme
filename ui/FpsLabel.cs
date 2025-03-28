using Godot;
using System.Globalization;

namespace Flamme.ui;

public partial class FpsLabel : Label
{
  private double TIMER_LIMIT = 0.3f;
  private double timer;
  
  public override void _Process(double delta)
  {
    timer += delta;
    if (timer > TIMER_LIMIT)
    {
      timer = 0.0;
      Text = Engine.GetFramesPerSecond().ToString(CultureInfo.InvariantCulture);
    }
  }
}