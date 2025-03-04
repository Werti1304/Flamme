using System;
using Flamme.common.enums;
using Godot;

namespace Flamme.entities.env.purse;

public abstract partial class PursePickup : Area2D
{
  public abstract Tuple<PurseContent, int> Pickup();
}