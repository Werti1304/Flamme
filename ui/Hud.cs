using Flamme.common.constant;
using Flamme.entities.player;
using Flamme.items;
using Flamme.testing;
using Godot;
using System;

namespace Flamme.ui;

public partial class Hud : CanvasLayer
{
  [ExportGroup("Textures")] 
  [Export] public Texture2D HeartEmpty;
  [Export] public Texture2D HeartFull;
  [Export] public Texture2D Heart3Qt;
  [Export] public Texture2D HeartHalf;
  [Export] public Texture2D Heart1Qt;
  [Export] public Texture2D AbsorptionHeartFull;
  [Export] public Texture2D AbsorptionHeart3Qt;
  [Export] public Texture2D AbsorptionHeartHalf;
  [Export] public Texture2D AbsorptionHeart1Qt;

  [ExportGroup("Meta")] 
  [Export] public Label ItemNameLabel;
  [Export] public Label ItemDescriptionLabel;
  [Export] public PurseDisplay PurseDisplay;
  [Export] private Container _healthRectContainer;
  private TextureRect[] _healthTextureRects = new TextureRect[10];
  [Export] public ColorRect Vignette;
  [Export] public MarginContainer MainContainer;
  [Export] public Minimap Minimap;
  [Export] private Timer _showCollectedItemtimer;
  
  public override void _Ready()
  {
    var idx = 0;
    foreach (var child in _healthRectContainer.GetChildren())
    {
      if (child is TextureRect rect)
      {
        _healthTextureRects[idx++] = rect;
      }
    }
    
    ExportMetaNonNull.Check(this);

    _showCollectedItemtimer.Timeout += HideCollectItem;
    
    HideCollectItem();
    Hide();
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
    
    // Also resets the  timer
    _showCollectedItemtimer.Start();
  }

  public void UpdateStats(PlayerStats playerStats)
  {
    UpdateHealth(playerStats);
  }

  public void UpdateHealth(PlayerStats playerStats)
  {
    // Normal health / health containers
    var fullContainers = playerStats.NormalHealth / 4;
    var lastNonEmptyContainer = playerStats.NormalHealth % 4;
    var textureRectIdx = 0;
    for (; textureRectIdx < playerStats.HealthContainers; textureRectIdx++)
    {
      if (fullContainers > 0)
      {
        // If full container
        _healthTextureRects[textureRectIdx].Texture = HeartFull;
        fullContainers--;
      }
      else if (lastNonEmptyContainer != 0)
      {
        _healthTextureRects[textureRectIdx].Texture = lastNonEmptyContainer switch
        {
          // If not full container
          3 => Heart3Qt,
          2 => HeartHalf,
          1 => Heart1Qt,
          _ => throw new ArgumentOutOfRangeException()
        };
        lastNonEmptyContainer = 0;
      }
      else
      {
        // Empty heart containers
        _healthTextureRects[textureRectIdx].Texture = HeartEmpty;
      }
    }
    
    // Absorption hearts
    // No need to check for overflow here, playerstats takes care of that
    fullContainers = playerStats.AbsorptionHealth / 4;
    lastNonEmptyContainer = playerStats.AbsorptionHealth % 4;

    var textureRectIdxOffset = textureRectIdx;
    textureRectIdx = 0;
    for (; textureRectIdx < fullContainers; textureRectIdx++)
    {
      _healthTextureRects[textureRectIdx + textureRectIdxOffset].Texture = AbsorptionHeartFull;
    }

    if (textureRectIdx + textureRectIdxOffset >= Universal.MaxPlayerHealthContainers)
      return;
    
    _healthTextureRects[textureRectIdx + textureRectIdxOffset].Texture = lastNonEmptyContainer switch
    {
      // If not full container
      3 => AbsorptionHeart3Qt,
      2 => AbsorptionHeartHalf,
      1 => AbsorptionHeart1Qt,
      0 => null,
      _ => throw new ArgumentOutOfRangeException()
    };
    textureRectIdx++;

    // Make sure the rest of the rects are empty
    for (; textureRectIdx + textureRectIdxOffset < Universal.MaxPlayerHealthContainers; textureRectIdx++)
    {
      _healthTextureRects[textureRectIdx + textureRectIdxOffset].Texture = null;
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