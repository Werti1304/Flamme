using Flamme.entities.player;
using Flamme.items;
using Flamme.testing;
using Godot;

namespace Flamme.ui;

public partial class Hud : CanvasLayer
{
  [ExportGroup("Textures")] 
  [Export] public Texture2D HeartFull;
  [Export] public Texture2D Heart3Qt;
  [Export] public Texture2D HeartHalf;
  [Export] public Texture2D Heart1Qt;

  [ExportGroup("Meta")] 
  [Export] public Label ItemNameLabel;
  [Export] public Label ItemDescriptionLabel;
  [Export] public StatsDisplay StatsDisplay;
  [Export] private TextureRect[] _healthTextureRects = new TextureRect[10];

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    HideCollectItem();
    Show();
  }

  public void HideCollectItem()
  {
    ItemNameLabel.Text = "";
    ItemDescriptionLabel.Text = "";
  }

  public void CollectItem(Item item)
  {
    ItemNameLabel.Text = item.Name;
    ItemDescriptionLabel.Text = item.Description;
    ItemNameLabel.Show();
    ItemDescriptionLabel.Show();
    GetTree().CreateTimer(5).Timeout += HideCollectItem;
  }

  public void UpdateStats(PlayerStats playerStats)
  {
    UpdateHealth(playerStats.Health);
    StatsDisplay.UpdateStats(playerStats);
  }
  
  public void UpdateHealth(int health)
  {
    // Max health
    if (health > 40)
    {
      health = 40;
    }

    var fullContainers = health / 4;

    for (var i = 0; i < fullContainers; i++)
    {
      if (_healthTextureRects[i].Texture != HeartFull)
      {
        _healthTextureRects[i].Texture = HeartFull;
      }
    } 

    var lastContainer = health % 4;

    _healthTextureRects[fullContainers].Texture = lastContainer switch
    {
      3 => Heart3Qt,
      2 => HeartHalf,
      1 => Heart1Qt,
      0 => null,
      _ => _healthTextureRects[fullContainers].Texture
    };

    for (int i = fullContainers + 1; i < (40 / 4); i++)
    {
      _healthTextureRects[i].Texture = null;
    }
  }
  
  private static Hud _instance;
  private static readonly object Padlock = new();
  
  public Hud()
  {
    _instance = this;
  }
  
  public static Hud Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }

}