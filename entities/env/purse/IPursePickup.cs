using System;
using Flamme.common.enums;

namespace Flamme.entities.env;

public interface IPursePickup
{
  public Tuple<PurseContent, int> Pickup();
}