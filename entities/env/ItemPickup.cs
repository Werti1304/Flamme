using Flamme.common.enums;
using Flamme.items;
using Flamme.testing;
using Godot;
using System;

namespace Flamme.entities.env;

public partial class ItemPickup : Area2D
{
  public enum ItemRetrievel
  {
    FromItemPool,
    FromId
  }
  
  [Export] public ItemRetrievel RetrievelMode = ItemRetrievel.FromItemPool;
  // TODO 3 Refactor from string to item id in one giant enum?
  [Export] public ItemId ItemId;
  [Export] public ItemLootPool ItemLootPool = ItemLootPool.Treasure;
  
  [ExportGroup("Meta")] 
  [Export] private Sprite2D _sprite;
  [Export] private CollisionShape2D _collisionShape;

  private Item _item;

  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    if (RetrievelMode == ItemRetrievel.FromId)
    {
      if (ItemId == ItemId.None)
      {
        GD.PushError("Tried to spawn item that was not set, setting to default item!");
        SetItem(ItemManager.Instance.DefaultItem);
      }
      else
      {
        SetFromId();
      }
    }
    else
    {
      SetFromPool();
    }
  }

  private void SetFromId()
  {
    if (ItemId == ItemId.None)
      return;
    
    var item = ItemManager.Instance.GetFromId(ItemId);

    if (item == null)
      return;
      
    SetItem(item);
  }

  private void SetFromPool()
  {
    SetItem(ItemManager.Instance.GetRandomFromPool(ItemLootPool));
  }

  public Item Pickup()
  {
    QueueFree();

    if (_item == null)
    {
      
      return ItemManager.Instance.DefaultItem;
    }
    return _item;
  }

  public void SetItem(Item item)
  {
    _item = item;

    if (item.SpriteFull == null)
    {
      GD.PushError($"Tried to place item {item.Name} down, which has no sprite!");
      return;
    }

    _sprite.Texture = item.SpriteFull;
  }

  public void HideItem()
  {
    SetDeferred(Area2D.PropertyName.Monitorable, false);
    _sprite.Hide();
  }

  public void ShowItem(bool enableMonitorable = true)
  {
    if (enableMonitorable)
    {
      SetDeferred(Area2D.PropertyName.Monitorable, true);
    }
    _sprite.Show();
  }
}