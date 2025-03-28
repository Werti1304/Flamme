using System;

namespace Flamme.entities.misc;

public interface IInterlinkable
{
  public event Action SendUnavailable;
  
  public void MakeUnavailable();
}
