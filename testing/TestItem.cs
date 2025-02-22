using Godot;
using System;
using System.Collections.Generic;

public partial class TestItem : Sprite2D
{
  public string getName()
  {
    return "Arcade Chip";
  }
  public string getDescription()
  {
    return "Example Item. +1 Heart and +1 Damage!";
  }

  public enum Stats
  {
    Health,
    Damage
  }

  public Dictionary<Stats, int> StatUpDict = new Dictionary<Stats, int>()
  {
    { Stats.Health, 4 },
    { Stats.Damage, 5 }
  };
}
