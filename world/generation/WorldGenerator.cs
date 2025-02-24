using Godot;

namespace Flamme.world.generation;

public partial class WorldGenerator : Node
{
  // TODO: Add actual generation logic here
  public RandomNumberGenerator NotSeedRng = new RandomNumberGenerator();

  public override void _Ready()
  {

  }

  public WorldGenerator()
  {
    _instance = this;
  }
  
  public static WorldGenerator Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static WorldGenerator _instance;
  private static readonly object Padlock = new object();
}