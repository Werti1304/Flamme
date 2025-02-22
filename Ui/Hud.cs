using Flamme.testing;
using Godot;

namespace Flamme.Ui;

public partial class Hud : CanvasLayer
{
  [ExportGroup("Textures")] 
  [Export] public Texture2D HeartFull;
  [Export] public Texture2D Heart3Qt;
  [Export] public Texture2D HeartHalf;
  [Export] public Texture2D Heart1Qt;

  [ExportGroup("Meta")] [Export] public TextureRect[] HealthTextureRects = new TextureRect[10];

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    Show();
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
      if (HealthTextureRects[i].Texture != HeartFull)
      {
        HealthTextureRects[i].Texture = HeartFull;
      }
    }

    var lastContainer = health % 4;

    switch (lastContainer)
    {
      case 3:
        HealthTextureRects[fullContainers].Texture = Heart3Qt;
        break;
      case 2:
        HealthTextureRects[fullContainers].Texture = HeartHalf;
        break;
      case 1:
        HealthTextureRects[fullContainers].Texture = Heart1Qt;
        break;
      case 0:
        HealthTextureRects[fullContainers].Texture = null;
        break;
    }

    for (int i = fullContainers + 1; i < (40 / 4); i++)
    {
      HealthTextureRects[i].Texture = null;
    }
  }
  
  private static Hud _instance;
  private static readonly object Padlock = new object();
  
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