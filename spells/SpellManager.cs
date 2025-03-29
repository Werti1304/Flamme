using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Flamme.spells;

public partial class SpellManager : Node
{
  // Stores spell id and spell as a dict, I don't trust an array alone
  // And Spell Registration in the Spell Class feels like bad form
  private readonly Dictionary<SpellId, Spell> _spellDict = new Dictionary<SpellId, Spell>();
  
  // When there is no other spell in the loot pool, pick this
  public Spell DefaultSpell;

  public void RegisterSpell(Spell spell)
  {
    _spellDict[spell.Id] = spell;
  }
  
  // For now, there are no spell lootpools
  public SpellManager()
  {
    _instance = this;
  }
  
  public Spell GetRandom()
  {
    var randomIndex = GD.RandRange(0, _spellDict.Count - 1);
    GD.Print($"Random index: {randomIndex}");
    var spell = _spellDict.ElementAt(randomIndex);
    
    return spell.Value;
  }

  public Spell GetFromId(SpellId id)
  {
    return _spellDict[id];
  }

  public static SpellManager Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static SpellManager _instance;
  private static readonly object Padlock = new object();
}
