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

public class SpellChangeEventArgs : EventArgs
{
  public entities.player.PlayableCharacter Character { get; init; }
  
  public SpellChangeEventArgs(entities.player.PlayableCharacter character)
  {
    Character = character;
  }
}
