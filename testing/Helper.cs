#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // Helper.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using System;

namespace Flamme.testing;

public static class Helper
{
  // Returns true if enumValue is power of 2 (or 0)
  // Used to check if bitflag enum is a single value or multiple
  public static bool IsPowerOf2(int enumValue)
  {
    return (enumValue & (enumValue - 1)) == 0;
  }
}
