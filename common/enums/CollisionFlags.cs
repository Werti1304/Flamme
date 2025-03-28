using System;

namespace Flamme.common.enums;

[Flags]
public enum CollisionFlags
{
  Walls = 0x1,
  Player = 0x2,
  Enemies = 0x4,
  Entities = 0x8
}
