using System.Runtime.InteropServices;
using Flamme.common.constant;
using Godot;

namespace Flamme.testing;

public static class ExportMetaNonNull
{
  public static bool Check(Node node)
  {
    if (!DebugToggles.CheckMetaNonNull)
      return true;
    
    bool hasErrors = false;
    
    // https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-get-property-list
    foreach (var nodeDict in node.GetPropertyList())
    {
      const string usage = "usage";
      const string hint = "hint";
      const string name = "name";
      
      var propertyUsage = (PropertyUsageFlags)(int)nodeDict[usage];
      // https://docs.godotengine.org/en/stable/classes/class_%40globalscope.html#enum-globalscope-propertyusageflags
      // If Storage or Editor usage flag is set, it's one that we need to check
      if ((propertyUsage & (PropertyUsageFlags.Storage | PropertyUsageFlags.Editor)) == 0)
      {
        continue;
      }
      
      var propertyHint = (PropertyHint)(int)nodeDict[hint];
      // Only check properties that are inherited by Node
      // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
      if ((propertyHint & PropertyHint.NodeType) == 0)
      {
          continue;
      }

      var propertyName = (string)nodeDict[name];
      var propertyObj = node.Get(property: propertyName);
      if (propertyObj.Obj != null)
      {
        continue;
      }

      // Exemptions go here
      if (propertyName == "shortcut_context")
      {
        continue;
      }
      
      GD.PushWarning($"Property {propertyName} from node {node} is null!");
      hasErrors = true;
    }
    return !hasErrors;
  }
}