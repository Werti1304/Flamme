using Flamme.items;
using Flamme.testing;
using Godot;
using System;
using System.Collections;

public partial class ItemPickup : Area2D
{
  // TODO 3 Refactor from string to item id in one giant enum?
  [Export] public string ItemName = "";
  
  [ExportGroup("Meta")] 
  [Export] private Sprite2D _sprite;
  [Export] private CollisionShape2D _collisionShape;

  private Item _item;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);
    
    _sprite.Hide();

    if (ItemName != "")
    {
      var item = ItemManager.Instance.GetFromName(ItemName);

      if (item == null)
        return;
      
      SetItem(item, true);
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
