using Flamme.common.enums;
using Flamme.common.helpers;
using Flamme.common.scenes;
using Flamme.items;
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
  [Export] public GpuParticles2D Particles2D;

  private Item _item;

  public static ItemPickup GetInstance(ItemId itemId)
  {
    var itemPickup = SceneLoader.Instance[SceneLoader.Scene.ItemPickup].Instantiate<ItemPickup>();
    itemPickup.RetrievelMode = ItemRetrievel.FromId;
    itemPickup.ItemId = itemId;
    itemPickup.ItemLootPool = 0;
    return itemPickup;
  }
  
  public static ItemPickup GetInstance(ItemLootPool itemLootPool)
  {
    var itemPickup = SceneLoader.Instance[SceneLoader.Scene.ItemPickup].Instantiate<ItemPickup>();
    itemPickup.RetrievelMode = ItemRetrievel.FromItemPool;
    itemPickup.ItemId = 0;
    itemPickup.ItemLootPool = itemLootPool;
    return itemPickup;
  }

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

  private bool _pickedUp = false;
  public Item Pickup()
  {
    if (_pickedUp)
      return null;
    _pickedUp = true;
    
    _sprite.Hide();
    Particles2D.OneShot = true;
    Particles2D.Restart();
    Particles2D.Finished += QueueFree;

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
    Particles2D.Emitting = false;
    _sprite.Hide();
  }

  public void ShowItem(bool enableMonitorable = true)
  {
    if (enableMonitorable)
    {
      SetDeferred(Area2D.PropertyName.Monitorable, true);
    }
    Particles2D.Emitting = true;
    _sprite.Show();
  }
}