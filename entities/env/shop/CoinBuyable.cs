using Flamme.entities.env;
using Godot;
using System;

public partial class Buyable : Area2D
{
  [Export] public int Price = 0;
  [Export] public IPursePickup PursePickup;

  [ExportGroup("Meta")] 
  [Export] public Label Label;
  
  public override void _Ready()
  {
    Label.Text = $"{Price},-";
  }
}
