using System;
using Flamme.common.enums;
using Flamme.common.helpers;
using Godot;

namespace Flamme.entities.player;

public partial class PlayerPurse : Node2D
{
  [Export] public int BaseCoins = 5; // /4 = Hearts
  [Export] public int BaseCrystals = 3;
  [Export] public int BaseKeys = 1;

  public int Coins { get; set; }
  public int Crystals { get; set; }
  public int Keys { get; set; }

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    Coins = BaseCoins;
    Crystals = BaseCrystals;
    Keys = BaseKeys;
  }

  public void Add(Tuple<PurseContent, int> purseContent)
  {
    switch (purseContent.Item1)
    {
      case PurseContent.Coin:
        Coins += purseContent.Item2;
        break;
      case PurseContent.Crystal:
        Crystals += purseContent.Item2;
        break;
      case PurseContent.Key:
        Keys += purseContent.Item2;
        break;
      default:
        throw new ArgumentException("Invalid PurseContent type");
    }
  }
}