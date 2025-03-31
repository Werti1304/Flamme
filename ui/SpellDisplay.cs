using Flamme.common.helpers;
using Flamme.common.input;
using Flamme.entities.player;
using Godot;
using System.Collections.Generic;
using System.Text;

namespace Flamme.ui;

public partial class SpellDisplay : RichTextLabel
{
  [ExportGroup("Action Icons")]
  [Export] private Texture2D _shootUpTexture;
  [Export] private Texture2D _shootRightTexture;
  [Export] private Texture2D _shootDownTexture;
  [Export] private Texture2D _shootLeftTexture;
  [Export] private Texture2D _shootUpActiveTexture;
  [Export] private Texture2D _shootRightActiveTexture;
  [Export] private Texture2D _shootDownActiveTexture;
  [Export] private Texture2D _shootLeftActiveTexture;

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
          AppendStat(sb, spell.Name, spell.ActionsNeeded, -1);
        }
        else
        {
          AppendStat(sb, spell.Name, spell.ActionsNeeded);
        }
      }
      else if (state == PlayerSpellBook.SpellState.Possible)
      {
        AppendStat(sb, spell.Name, spell.ActionsNeeded, book.ActionsNeededIdx);
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

  /// <summary>
  /// 
  /// </summary>
  /// <param name="sb"></param>
  /// <param name="name"></param>
  /// <param name="actions"></param>
  /// <param name="possibleIdx">
  /// 0: Inactive
  /// >0: Possible
  /// -1: Spell Casting Inactive in general</param>
  private void AppendStat(StringBuilder sb, string name, IEnumerable<PlayerInputMap.Action> actions, int possibleIdx = 0)
  {
    if (possibleIdx == -1)
    {
      sb.Append($"[color=Gray]{name}[/color]\n");
      return;
    }
    sb.Append($"{name} ");

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