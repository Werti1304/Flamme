using Godot;
using System;
using Flamme.ui;

public partial class HealthBar : TextureProgressBar
{
  // public override void _Ready()
  // {
  //   Reparent(Hud.Instance.HealthBarContainer, false);
  //   Owner = Hud.Instance;
  // }

  public void OnHealthChanged(Enemy enemy)
  {
    if (enemy.MaxHealth == 0)
    {
      Value = 100.0f;
    }
    else
    {
      Value = enemy.Health / enemy.MaxHealth * 100.0f;
    }
  }

  public void OnDeath()
  {
    Value = 0;
    var tween = GetTree().CreateTween();
    tween.TweenProperty(this, CanvasItem.PropertyName.Modulate.ToString(), Colors.Transparent, 1.0f);
    tween.TweenCallback(Callable.From(QueueFree));
  }
}
