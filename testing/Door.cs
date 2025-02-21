using Flamme.testing;
using Godot;
using System;

public partial class Door : StaticBody2D
{
  [ExportGroup("Meta")] 
  [Export] public Sprite2D ClosedSprite;
  [Export] public CollisionShape2D ClosedShape;
  
  private bool _isOpen = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public void Open()
  {
    if (_isOpen)
    {
      return;
    }
    _isOpen = true;

    ClosedShape.SetDeferred("disabled", true);
    ClosedSprite.Visible = false;
  }

  public void Close()
  {
    if (!_isOpen)
    {
      return;
    }
    _isOpen = false;
    ClosedShape.SetDeferred("disabled", false);
    ClosedSprite.Visible = true;
  }
}
