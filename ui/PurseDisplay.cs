using Godot;
using System;
using System.Text;
using Flamme.entities.player;
using Flamme.testing;

public partial class PurseDisplay : RichTextLabel
{
  [Export] public Texture2D CoinIcon;
  [Export] public Texture2D CrystalIcon;
  [Export] public Texture2D KeyIcon;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }
  
  public void UpdatePurse(PlayerPurse purse)
  {
    Text = BuildStatString(purse);
  }
  
  private string BuildStatString(PlayerPurse purse)
  {
    var sb = new StringBuilder();
    AppendStat(sb, CoinIcon, purse.Coins);
    AppendStat(sb, CrystalIcon, purse.Crystals);
    AppendStat(sb, KeyIcon, purse.Keys);
    return sb.ToString();
  }

  private static void AppendStat(StringBuilder sb, Texture2D icon, float value)
  {
    if (icon != null)
    {
      sb.Append($"[img=16]{icon.ResourcePath}[/img] {value}\n");
    }
  }
}
