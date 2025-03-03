using Godot;
using System;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.entities.env;

public partial class Key : Area2D, IPursePickup
{
  public Tuple<PurseContent, int> Pickup()
  {
    QueueFree();
    return new Tuple<PurseContent, int>(PurseContent.Key, 1);
  }
}
