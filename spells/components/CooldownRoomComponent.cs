using System;

namespace Flamme.spells.components;

public class CooldownRoomComponent
{
  public int Cooldown;
  public int CooldownCounter { get; private set; }
  
  public CooldownRoomComponent(int cooldown)
  {
    Cooldown = cooldown;
  }

  public void Reset()
  {
    CooldownCounter = 0;
  }

  public void Tick()
  {
    CooldownCounter++;
  }

  public bool IsFinished()
  {
    var cooldownFinished = CooldownCounter >= Cooldown;

    if (cooldownFinished)
    {
      CooldownCounter = 0;
      return true;
    }
    return false;
  }
}
