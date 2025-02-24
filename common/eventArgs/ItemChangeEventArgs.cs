#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // ItemAddedEventArgs.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using System;

namespace Flamme.common.eventArgs;

public class ItemChangeEventArgs : EventArgs
{
  public PlayableCharacter Character { get; init; }
  
  public ItemChangeEventArgs(PlayableCharacter character)
  {
    Character = character;
  }
}
