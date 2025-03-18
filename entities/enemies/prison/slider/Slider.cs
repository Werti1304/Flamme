using Godot;
using System;

public partial class Slider : Enemy
{
  [ExportGroup("Meta")]
  [Export] public HealthBar HealthBar;

  public override void _Ready()
  {
    base._Ready();
    
    HealthChanged += HealthBar.OnHealthChanged;
    
    HealthBar.OnHealthChanged(this);
  }
}
