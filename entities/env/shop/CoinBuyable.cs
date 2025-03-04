using Flamme.common.constant;
using Flamme.testing;
using Godot;
using System;

namespace Flamme.entities.env.shop;

public partial class CoinBuyable : Area2D
{
  [Export] public int Price = 0;
  private Area2D _selling;

  [ExportGroup("Meta")] 
  [Export] public Label Label;
  
  public override void _Ready()
  {
    // Search for sellable node in node children
    if (_selling == null)
    {
      foreach (var childNodes in GetChildren())
      {
        if (childNodes is not Area2D area2D)
          continue;
        _selling = area2D;
        break;
      }
    }
    
    ExportMetaNonNull.Check(this);
    
    Label.Text = $"{Price},-";
    
    _selling.Monitorable = false;
  }

  public void Buy(PlayableCharacter player)
  {
    if (_selling == null || _selling.IsQueuedForDeletion())
    {
      QueueFree();
      return;
    }
    
    _selling.SetDeferred(Area2D.PropertyName.Monitorable, true);
    // Slightly cursed but best way to instantly trigger event
    player.InteractionArea.EmitSignal(Area2D.SignalName.AreaEntered, _selling);
    Label.Text = "";
  }
}