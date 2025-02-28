
using Flamme.items;
using Flamme.world.rooms;
using Godot;
using System;

[Tool]
public partial class AllItems : Room
{
  [Export] public PackedScene ItemPickupScene;
  public override void _Ready()
  {
    base._Ready();

    if (Engine.IsEditorHint())
      return;

    var currentVec = new Vector2(128, 128);
    
    foreach (ItemId itemid in Enum.GetValues(typeof(ItemId)))
    {
      if(itemid == ItemId.None)
        continue;
      
      var itemPickup = ItemPickupScene.Instantiate<ItemPickup>();
      AddChild(itemPickup);
      itemPickup.Owner = this;
      itemPickup.SetItem(ItemManager.Instance.GetFromId(itemid), true);
      itemPickup.Position = currentVec;
      currentVec += new Vector2(2 * 32, 0);
    }
  }
}
