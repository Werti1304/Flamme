namespace Flamme.spells.components;

public class UptimeComponent
{
  public double Uptime { get; private set; }
  private double _lifeTime = 0;

  public UptimeComponent(double uptime)
  {
    Uptime = uptime;
  }

  public bool IsAlive()
  {
    return _lifeTime < Uptime;
  }

  public void Reset()
  {
    _lifeTime = 0;
  }
  
  public void Tick(double deltaTime)
  {
    _lifeTime += deltaTime;
  }
}
