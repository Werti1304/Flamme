using Godot;
using PursePickup = Flamme.entities.env.purse.PursePickup;

namespace Flamme.entities.env.shop;

public partial class CoinBuyable : Area2D
{
  [Export] public int Price = 0;
  [Export] private PursePickup _pursePickup;

  [ExportGroup("Meta")] 
  [Export] public Label Label;
  
  public override void _Ready()
  {
    Label.Text = $"{Price},-";

    _pursePickup.Monitoring = false;
    _pursePickup.Monitorable = false;
  }

  public PursePickup Buy()
  {
    QueueFree();
    return _pursePickup;
  }
}