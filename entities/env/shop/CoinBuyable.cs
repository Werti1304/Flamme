using Flamme.common.constant;
using Flamme.entities.env.health;
using Flamme.entities.env.purse;
using Flamme.testing;
using Godot;
using System;

namespace Flamme.entities.env.shop;

public partial class CoinBuyable : Area2D
{
  [Export] public int Price = 0;
  private Node2D _selling;

  [ExportGroup("Meta")] 
  [Export] public Label Label;
  
  public override void _Ready()
  {
    // Search for sellable node in node children
    if (_selling == null)
    {
      foreach (var childNodes in GetChildren())
      {
        if (childNodes is Area2D area2D)
        {
          area2D.Monitorable = false;
          _selling = area2D;
          break;
        }
        if (childNodes is RigidBody2D rigidBody2D)
        {
          _selling = rigidBody2D;
          switch (_selling)
          {
            case HealthPickup healthPickup:
              healthPickup.CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
              break;
            case PursePickup pursePickup:
              pursePickup.CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
              break;
            default:
              GD.PushError($"Tried to spawn a coin buyable with {_selling.GetType().Name} as sellable node!");
              break;
          }
          break;
        }
      }
    }
    
    ExportMetaNonNull.Check(this);
    
    Label.Text = $"{Price},-";

    if (_selling == null)
    {
      GD.PushError($"No sellable node found in {Name}!");
    }
  }

  public void Buy(PlayableCharacter player)
  {
    if (_selling == null || !IsInstanceValid(_selling))
    {
      QueueFree();
      return;
    }

    if (_selling is Area2D)
    {
      _selling.SetDeferred(Area2D.PropertyName.Monitorable, true);
      // Slightly cursed but best way to instantly trigger event
      player.InteractionArea.EmitSignal(Area2D.SignalName.AreaEntered, _selling);
    }
    else if (_selling is RigidBody2D)
    {
      // Slightly cursed but best way to instantly trigger event
      player.InteractionArea.EmitSignal(Area2D.SignalName.BodyEntered, _selling);
    }

    Label.Text = "";
    QueueFree();
  }
}