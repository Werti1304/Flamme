using System;

namespace Flamme.common.enums;

[Flags]
public enum RoomExit
{
  North  = 0x1,   // 1
  South  = 0x2,   // 2
  West   = 0x4,   // 4
  East   = 0x8,   // 8
  North2 = 0x10, // 16
  South2 = 0x20, // 32
  West2  = 0x40, // 64
  East2  = 0x80, // 128
  North3 = 0x100, // 256
  South3 = 0x200, // 512
  West3  = 0x400, // 1024
  East3  = 0x800,  // 2048
}
