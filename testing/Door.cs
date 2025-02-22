using Flamme.testing;
using Godot;

public partial class Door : StaticBody2D
{
  [ExportGroup("Meta")] 
  [Export] public Sprite2D ClosedSprite;
  [Export] public Sprite2D ClosedSprite2;
  [Export] public CollisionShape2D ClosedShape;
  
  private bool _isOpen = false;
  private bool _isLocked = false;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
  }

  public void Open()
  {
    if (_isOpen || _isLocked)
    {
      return;
    }
    _isOpen = true;

    ClosedShape.SetDeferred("disabled", true);
    ClosedSprite.Visible = false;
    ClosedSprite2.Visible = false;
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
    ClosedSprite2.Visible = true;
  }

  public void Lock()
  {
    Close();
    _isLocked = true;
  }

  public void Unlock()
  {
    _isLocked = false;
  }
}
