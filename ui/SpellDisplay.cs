using Flamme.common.helpers;
using Flamme.common.input;
using Flamme.entities.player;
using Flamme.spells;
using Godot;
using System.Collections.Generic;
using System.Text;

namespace Flamme.ui;

public partial class SpellDisplay : RichTextLabel
{
  [ExportGroup("Action Icons")] [Export] private Texture2D _shootUpTexture;
  [Export] private Texture2D _shootRightTexture;
  [Export] private Texture2D _shootDownTexture;
  [Export] private Texture2D _shootLeftTexture;
  [Export] private Texture2D _shootUpActiveTexture;
  [Export] private Texture2D _shootRightActiveTexture;
  [Export] private Texture2D _shootDownActiveTexture;
  [Export] private Texture2D _shootLeftActiveTexture;
  [Export] private Texture2D _crystal;
  [Export] private Texture2D _crystalCanNotAfford;

  private readonly Dictionary<PlayerInputMap.Action, Texture2D> _actionIcons =
    new Dictionary<PlayerInputMap.Action, Texture2D>();

  private readonly Dictionary<PlayerInputMap.Action, Texture2D> _actionActiveIcons =
    new Dictionary<PlayerInputMap.Action, Texture2D>();

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    _actionIcons[PlayerInputMap.Action.ShootUp] = _shootUpTexture;
    _actionIcons[PlayerInputMap.Action.ShootRight] = _shootRightTexture;
    _actionIcons[PlayerInputMap.Action.ShootDown] = _shootDownTexture;
    _actionIcons[PlayerInputMap.Action.ShootLeft] = _shootLeftTexture;

    _actionActiveIcons[PlayerInputMap.Action.ShootUp] = _shootUpActiveTexture;
    _actionActiveIcons[PlayerInputMap.Action.ShootRight] = _shootRightActiveTexture;
    _actionActiveIcons[PlayerInputMap.Action.ShootDown] = _shootDownActiveTexture;
    _actionActiveIcons[PlayerInputMap.Action.ShootLeft] = _shootLeftActiveTexture;
  }

  public void Update(PlayerSpellBook book)
  {
    Text = BuildStatString(book);
  }

  private string BuildStatString(PlayerSpellBook book)
  {
    var sb = new StringBuilder();

    foreach (var spellPair in book.Spells)
    {
      var spell = spellPair.Key;
      var state = spellPair.Value;

      if (state == PlayerSpellBook.SpellState.Ready)
      {
        if (!book.IsListening)
        {
          AppendSpell(sb, spell, book, spell.ActionsNeeded, -1);
        }
        else
        {
          AppendSpell(sb, spell, book, spell.ActionsNeeded);
        }
      }
      else if (state == PlayerSpellBook.SpellState.Possible)
      {
        AppendSpell(sb, spell, book, spell.ActionsNeeded, book.ActionsNeededIdx);
      }
      else if (state == PlayerSpellBook.SpellState.OnCoolDown)
      {
        if (spell.CooldownRoomComponent == null)
        {
          sb.Append($"[color=Dimgray]{spell.Name}[/color]\n");
        }
        else
        {
          sb.Append($"[color=Dimgray]{spell.Name} [[/color]");

          for (var i = 0; i < spell.CooldownRoomComponent.Cooldown; i++)
          {
            if (spell.CooldownRoomComponent.CooldownCounter > i)
            {
              sb.Append($"[color=Orange]|[/color]");
            }
            else
            {
              sb.Append($"[color=Dimgray]|[/color]");
            }
          }
          sb.Append($"[color=Dimgray]][/color]\n");
        }
      }
      else if (state == PlayerSpellBook.SpellState.Casting)
      {
        sb.Append($"[color=Orange]{spell.Name}[/color]\n");
      }
    }
    return sb.ToString();
  }

  /// <param name="sb"></param>
  /// <param name="spell"></param>
  /// <param name="book"></param>
  /// <param name="actions"></param>
  /// <param name="possibleIdx">
  /// 0: Inactive
  /// >0: Possible
  /// -1: Spell Casting Inactive in general</param>
  private void AppendSpell(StringBuilder sb, Spell spell, PlayerSpellBook book, IEnumerable<PlayerInputMap.Action> actions,
    int possibleIdx = 0)
  {
    if (spell.ChargeCrystalCost > book.AvailableCrystals)
    {
      sb.Append($"[color=Dimgray]{spell.Name} [/color]");
    }
    else if (possibleIdx == -1)
    {
      sb.Append($"[color=Gray]{spell.Name} [/color]");
    }
    else
    {
      sb.Append($"{spell.Name} ");
    }

    for (var k = 0; k < spell.ChargeCrystalCost; k++)
    {
      if (k + 1 <= book.AvailableCrystals)
      {
        sb.Append($"[img=16]{_crystal.ResourcePath}[/img]");
      }
      else
      {
        sb.Append($"[img=16]{_crystalCanNotAfford.ResourcePath}[/img]");
      }
    }

    if (possibleIdx == -1 || spell.ChargeCrystalCost > book.AvailableCrystals)
    {
      sb.Append("\n");
      return;
    }

    var i = 0;

    foreach (var a in actions)
    {
      if (i < possibleIdx)
      {
        sb.Append($"[img=16]{_actionActiveIcons[a].ResourcePath}[/img]");
      }
      else
      {
        sb.Append($"[img=16]{_actionIcons[a].ResourcePath}[/img]");
      }
      i++;
    }
    sb.Append("\n");
  }
}