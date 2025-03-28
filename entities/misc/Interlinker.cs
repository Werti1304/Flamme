using Godot;
using Godot.Collections;

namespace Flamme.entities.misc;

public partial class Interlinker : Node2D
{
  // These have to implement IInterlinkable!!!
  // Not specifiable through GoDot Exports unfortunately
  // Fills automatically if count is 0
  [Export] public Array<Node2D> InterlinkedNodes = new Array<Node2D>();

  public override void _Ready()
  {
    if (InterlinkedNodes.Count == 0)
    {
      foreach (var child in GetChildren())
      {
        if (child is IInterlinkable)
        {
          InterlinkedNodes.Add(child as Node2D);
        }
      }
    }
    
    if (InterlinkedNodes.Count == 0)
    {
      GD.PushWarning($"No InterlinkedNodes found in Interlinker {Name} at coords {GlobalPosition}, parent {GetParent()}");
    }

    foreach (var interlinked in InterlinkedNodes)    
    {
      if (interlinked is IInterlinkable interlinkable)
      {
        interlinkable.SendUnavailable += InterlinkableOnSendUnavailable;
      }
      else
      {
        GD.PushWarning(
          $"InterlinkedNode {interlinked.Name} in Interlinker {Name} at coords {GlobalPosition} is not IInterlinkable");
        return;
      }
    }
  }

  private void InterlinkableOnSendUnavailable()
  {
    foreach (var interlinked in InterlinkedNodes)
    {
      if (interlinked is IInterlinkable interlinkable)
      {
        interlinkable.MakeUnavailable();
      }
      else
      {
        GD.PushWarning(
          $"InterlinkedNode {interlinked.Name} in Interlinker {Name} at coords {GlobalPosition} is not IInterlinkable");
        return;
      }
    }
  }
}
