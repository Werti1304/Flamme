using Flamme.entities.player;
using Flamme.testing;
using Godot;
using System;
using System.Text;

public partial class StatsDisplay : RichTextLabel
{
  [Export] public Texture2D DamageIcon;
  [Export] public Texture2D FireRateIcon;
  [Export] public Texture2D SpeedIcon;
  [Export] public Texture2D RangeIcon;
  [Export] public Texture2D ShotSpeedIcon;
  [Export] public Texture2D LuckIcon;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public void UpdateStats(PlayerStats stats)
  {
    Text = BuildStatString(stats);
  }
  
  private string BuildStatString(PlayerStats stats)
  {
    var sb = new StringBuilder();
    AppendStat(sb, DamageIcon, (float)Math.Round(stats.Damage, 2));
    AppendStat(sb, FireRateIcon, stats.FireRate);
    AppendStat(sb, SpeedIcon, stats.Speed);
    AppendStat(sb, RangeIcon, stats.Range);
    AppendStat(sb, ShotSpeedIcon, stats.ShotSpeed);
    AppendStat(sb, LuckIcon, stats.Luck);
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
