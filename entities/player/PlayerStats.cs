#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // PlayerStats.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Godot;

namespace Flamme.entities.player;

public partial class PlayerStats : Node2D
{
  [Export] public int BaseHealth = 12;        // /4 = Hearts
  [Export] public int BaseAbsorption = 0;
  [Export] public int BaseSpeed = 50;         // px/sec?
  [Export] public int BaseDamage = 3;         // Damage against enemies
  [Export] public int FireRate = 60;          // /min?
  [Export] public int ShotSpeed = 100;        // px/sec?
  [Export] public int ShotSize = 6;           // px radius? Different bullets for differen sizes
  [Export] public int Luck = 3;               // Chance for more loot / better items
  [Export] public int Mana = 100;             // Idk yet

}
