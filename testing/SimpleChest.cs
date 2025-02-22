using Flamme.testing;
using Godot;
using System;

public interface ITouchInteractable
{
  /// <summary>
  /// Tries to interact with entity by "touching" with playable character
  /// This is for chest, etc., for something the player should pick up,
  /// use ICollectable
  /// </summary>
  /// <param name="simpleCharacter">Character who interacts</param>
  /// <returns>Whether it was able to interact
  /// If already interacted -> false</returns>
  public bool TryInteract(SimpleCharacter simpleCharacter);
}

public partial class SimpleChest : RigidBody2D, ITouchInteractable
{
  public bool Closed = true;

  [ExportGroup("Textures")] 
  [Export] public Texture2D ClosedTexture;
  [Export] public Texture2D OpenTexture;

  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public TestItem HeldItem;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    HeldItem.Hide();
  }

  public bool TryInteract(SimpleCharacter simpleCharacter)
  {
    if (!Closed)
      return false;

    Open();
    return true;
  }

  public TestItem PickupItem()
  {
    if (HeldItem == null)
    {
      return null;
    }
    HeldItem.Hide();
    var retItem = HeldItem;
    HeldItem = null;
    
    return retItem;
  }

  public void Open()
  {
    Closed = false;
    Sprite.Texture = OpenTexture;
    HeldItem.Show();
  }
}
