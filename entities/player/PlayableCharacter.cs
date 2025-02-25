using Flamme.items;
using Godot;
using System;
using System.Collections.Generic;
using Flamme.entities.player;
using Flamme.ui;

public partial class PlayableCharacter : CharacterBody2D
{
  [ExportGroup("Meta")] 
  [Export] public PlayerStats Stats;
  public List<Item> HeldItems = new List<Item>();

  public void PickupItem(Item item)
  {
    HeldItems.Add(item);
    Stats.Update(HeldItems);
    Hud.Instance.UpdateHealth(Stats.HealthContainers);
  }
}
