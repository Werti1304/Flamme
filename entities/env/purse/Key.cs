using Godot;
using System;
using System.Collections.Generic;
using Flamme.common.enums;
using Flamme.entities.env;
using PursePickup = Flamme.entities.env.purse.PursePickup;

public partial class Key : PursePickup
{
  public override Tuple<PurseContent, int> Pickup()
  {
    QueueFree();
    return new Tuple<PurseContent, int>(PurseContent.Key, 1);
  }
}
