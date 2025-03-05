using Flamme.common.enums;
using Flamme.items;
using Godot;

namespace Flamme.testing;

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
  [Export] public Sprite2D HeldItemSprite;

  private Item _heldItem;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    HeldItemSprite.Hide();
    _heldItem = ItemManager.Instance.GetRandomFromPool(ItemLootPool.Chest);
    HeldItemSprite.Texture = _heldItem.SpriteFull;
  }
  
  public bool TryInteract(PlayableCharacter character)
  {
    if (!Closed)
      return false;

    Open();
    return true;
  }

  public bool TryInteract(SimpleCharacter simpleCharacter)
  {
    if (!Closed)
      return false;

    Open();
    return true;
  }

  public Item PickupItem()
  {
    if (_heldItem == null)
    {
      return null;
    }
    HeldItemSprite.Hide();
    var itemToPickup = _heldItem;
    _heldItem = null;
    
    return itemToPickup;
  }

  public void Open()
  {
    Closed = false;
    Sprite.Texture = OpenTexture;
    HeldItemSprite.Show();
  }
}