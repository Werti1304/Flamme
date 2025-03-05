using Flamme.common.enums;
using Flamme.items;
using Flamme.testing;
using Godot;

namespace Flamme.entities.env;

public partial class ItemPickup : Area2D
{
  // TODO 3 Refactor from string to item id in one giant enum?
  [Export] private ItemId _itemId;
  
  [ExportGroup("Meta")] 
  [Export] private Sprite2D _sprite;
  [Export] private CollisionShape2D _collisionShape;

  private Item _item;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    _sprite.Hide();

    if (_itemId != ItemId.None)
    {
      var item = ItemManager.Instance.GetFromId(_itemId);

      if (item == null)
        return;
      
      SetItem(item, true);
    }
    else
    {
      SetItem(ItemManager.Instance.GetRandomFromPool(ItemLootPool.Treasure, false), true);
    }
  }

  public Item Pickup()
  {
    QueueFree();
    return _item;
  }

  public void SetItem(Item item, bool show)
  {
    _item = item;

    if (item.SpriteFull == null)
    {
      GD.PushError($"Tried to place item {item.Name} down, which has no sprite!");
      return;
    }

    _sprite.Texture = item.SpriteFull;

    if (show)
    {
      ShowItem();
    }
  }

  public void ShowItem()
  {
    _collisionShape.Disabled = false;
    _sprite.Show();
  }
}