
using Godot;

namespace Flamme.world.rooms;

public class RoomLoot
{
  // Placeholder
  // Todo: Coinheap class - first seperated coins into gold, silver, copper
  // Then makes a pile of gold (highest in the top middle, stack other ones left and right in front of it)
  public int coins;
  
  public RoomLoot()
  {
    coins = GD.RandRange(0, 10);
  }
}
