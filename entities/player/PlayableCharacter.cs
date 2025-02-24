using Flamme.items;
using Godot;
using System;
using System.Collections.Generic;

public partial class PlayableCharacter : CharacterBody2D
{
  
  public List<Item> HeldItems = new List<Item>();

  public void PickupItem(Item item)
  {
    
  }
}
