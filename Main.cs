#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // Main.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Flamme.items;
using Godot;

namespace Flamme;

public partial class Main : Node
{
  // TODO Item Inventory
  // TODO Item List ingame
  // TODO Minimap (After world gen?)
  // TODO Stats in HUD
  public override void _Ready()
  {
    // Use GD.[...] for seeded stuff -> level Layout, Room Layout, Items, Enemies, Chest Contents, etc.
    // GD.Seet() For when we want to use seed
    GD.Randomize();
    StatUpItems.RegisterStatUpItems();
  }
}
