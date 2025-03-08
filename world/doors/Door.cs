using Flamme.common.enums;
using Flamme.entities.player;
using Godot;
using System;

public partial class Door : StaticBody2D
{
  public enum DoorType
  {
    Bars,
    Boss,
    Gold,
    Shop,
  }
  
  [Export] private DoorType _type = DoorType.Bars;
  
  [ExportGroup("Textures")]
  [Export] public AtlasTexture BarClosedTexture;
  [Export] public AtlasTexture BarOpenTexture;
  [Export] public AtlasTexture BossClosedTexture;
  [Export] public AtlasTexture BossOpenTexture;
  [Export] public AtlasTexture GoldClosedTexture;
  [Export] public AtlasTexture GoldOpenTexture;
  [Export] public AtlasTexture ShopClosedTexture;
  [Export] public AtlasTexture ShopOpenTexture;
  
  [ExportGroup("Meta")] 
  [Export] public Sprite2D Sprite;
  [Export] public Sprite2D SpriteMirrored;
  [Export] public CollisionShape2D CollisionShape;
  
  private bool _isOpen = false;
  private bool _isLocked = false;
  private readonly object _padlock = new object();
  
  private AtlasTexture _openTexture;
  private AtlasTexture _closedTexture;

  public void SetPerRoomTypes(RoomType roomType1, RoomType roomType2)
  {
    if (roomType1 == RoomType.Treasure || roomType2 == RoomType.Treasure)
    {
      _type = DoorType.Gold;
    }
    else if (roomType1 == RoomType.Boss || roomType2 == RoomType.Boss)
    {
      _type = DoorType.Boss;
    }
    else if (roomType1 == RoomType.Shop || roomType2 == RoomType.Shop)
    {
      _type = DoorType.Shop;
    }
    else
    {
      _type = DoorType.Bars;
    }
  }

  public override void _Ready()
  {
    switch (_type)
    {
      case DoorType.Bars:
        _openTexture = BarOpenTexture;
        _closedTexture = BarClosedTexture;
        break;
      case DoorType.Boss:
        _openTexture = BossOpenTexture;
        _closedTexture = BossClosedTexture;
        break;
      case DoorType.Gold:
        _openTexture = GoldOpenTexture;
        _closedTexture = GoldClosedTexture;
        break;
      case DoorType.Shop:
        _openTexture = ShopOpenTexture;
        _closedTexture = ShopClosedTexture;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
    
    Close();
  }

  public virtual void Lock()
  {
    lock (_padlock)
    {
      _isLocked = true;
    }
    Close();
  }

  public void Unlock()
  {
    lock (_padlock)
    {
      _isLocked = false;
    }
  }

  public void OpenByClearingRoom()
  {
    if (_isLocked || _type != DoorType.Bars)
    {
      return;
    }
    Open();
  }

  public virtual bool TryOpen(PlayableCharacter player)
  {
    if( _isLocked)
      return false;
    Open();
    return true;
  }

  protected virtual void Open()
  {
    if( _isLocked)
      return;

    lock (_padlock)
    {
      _isOpen = true;
      Sprite.Texture = _openTexture;
      SpriteMirrored.Texture = _openTexture;
      CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }
  }

  public virtual void Close()
  {
    lock (_padlock)
    {
      _isOpen = false;
      Sprite.Texture = _closedTexture;
      SpriteMirrored.Texture = _closedTexture;
      CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }
  }
}
