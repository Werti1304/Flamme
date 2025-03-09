using Godot;

namespace Flamme.projectiles;

public static class ProjectileHelper
{
  // https://docs.godotengine.org/en/latest/tutorials/math/beziers_and_curves.html#quadratic-bezier
  public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
  {
    var q0 = p0.Lerp(p1, t);
    var q1 = p1.Lerp(p2, t);
    
    var r = q0.Lerp(q1, t);
    return r;
  }
  
  public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
  {
    var q0 = p0.Lerp(p1, t);
    var q1 = p1.Lerp(p2, t);
    var q2 = p2.Lerp(p3, t);

    var r0 = q0.Lerp(q1, t);
    var r1 = q1.Lerp(q2, t);

    var s = r0.Lerp(r1, t);
    return s;
  }
}
