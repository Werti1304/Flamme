using Flamme.common.constant;
using Flamme.common.enums;
using Flamme.testing;
using Flamme.world.doors;
using Flamme.world.generation;
using Flamme.world.rooms;
using Godot;
using System;
// ReSharper disable HeuristicUnreachableCode

public partial class Door : Node2D
{
  public enum DoorType
  {
    Bars,
    Boss,
    Gold,
    Shop,
  }
  
  [Export] private DoorType _type = DoorType.Bars;
  [Export] private DoorMarker _doorMarker1;
  [Export] private DoorMarker _doorMarker2;
  [Export] public Room Room1;
  [Export] public Room Room2;
  
  [ExportGroup("Textures")]
  [Export] public AtlasTexture BarClosedTexture;
  [Export] public AtlasTexture BarOpenTexture;
  [Export] public AtlasTexture BossClosedTexture;
  [Export] public AtlasTexture BossOpenTexture;
  [Export] public AtlasTexture GoldClosedTexture;
  [Export] public AtlasTexture GoldOpenTexture;
  [Export] public AtlasTexture ShopClosedTexture;
  [Export] public AtlasTexture ShopOpenTexture;
  
  private bool _isOpen = false;
  private bool _isLocked = false;
  private bool _isLockedByKey = false;
  
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
    if (_doorMarker1 == null || _doorMarker2 == null)
    {
      GD.PushWarning("Did you forget to set the door markers?");
      return;
    }

    if (Room1 == null || Room2 == null)
    {
      GD.PushWarning("Door has at least one room not set!");
      return;
    }
    
    ExportMetaNonNull.Check(this);
    
    switch (_type)
    {
      case DoorType.Bars:
        _doorMarker1.TextureOpen = BarOpenTexture;        
        _doorMarker2.TextureOpen = BarOpenTexture;
        _doorMarker1.TextureClosed = BarClosedTexture;
        _doorMarker2.TextureClosed = BarClosedTexture;
        break;
      case DoorType.Boss:
        _doorMarker1.TextureOpen = BossOpenTexture;        
        _doorMarker2.TextureOpen = BossOpenTexture;
        _doorMarker1.TextureClosed = BossClosedTexture;
        _doorMarker2.TextureClosed = BossClosedTexture;
        break;
      case DoorType.Gold:
        _doorMarker1.TextureOpen = GoldOpenTexture;        
        _doorMarker2.TextureOpen = GoldOpenTexture;
        _doorMarker1.TextureClosed = GoldClosedTexture;
        _doorMarker2.TextureClosed = GoldClosedTexture;
        _isLockedByKey = true;
        break;
      case DoorType.Shop:
        _doorMarker1.TextureOpen = ShopOpenTexture;        
        _doorMarker2.TextureOpen = ShopOpenTexture;
        _doorMarker1.TextureClosed = ShopClosedTexture;
        _doorMarker2.TextureClosed = ShopClosedTexture;
        _isLockedByKey = true;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
    
    Open();
    
    _doorMarker1.Teleport += OnTeleportDoorMarker1;
    _doorMarker2.Teleport += OnTeleportDoorMarker2;
  }
  
  private void OnTeleportDoorMarker1(PlayableCharacter character)
  {
    Level.Current.Teleport(Room1, Room2, _doorMarker2.TeleportPoint.GlobalPosition);
  }
  
  private void OnTeleportDoorMarker2(PlayableCharacter character)
  {
    Level.Current.Teleport(Room2, Room1, _doorMarker1.TeleportPoint.GlobalPosition); 
  }

  public virtual void Lock()
  {
    _isLocked = true;
    Close();
  }

  public void Unlock()
  {
    _isLocked = false;
  }

  public void OpenByClearingRoom()
  {
    Unlock();
    if (_isLockedByKey)
    {
      return;
    }
    Open();
  }

  public virtual bool TryOpen(PlayableCharacter player)
  {
    if (_isLocked)
      return false;
    
    if (_isLockedByKey)
    {
      if (player.Purse.Keys >= 1)
      {
        player.Purse.Keys--;
        Unlock();
      }
      else
      {
        return false;
      }
    }
    Open();
    return true;
  }

  protected virtual void Open()
  {
    if (_isLocked)
      return;

    _isOpen = true;
    
    _doorMarker1.Open();
    _doorMarker2.Open();
  }

  public virtual void Close()
  {
    if (DebugToggles.DoorsAlwaysOpen)
      
    #pragma warning disable CS0162 // Unreachable code detected
    {
      Open();
      return;
    }
    #pragma warning restore CS0162 // Unreachable code detected
    
    _isOpen = false;
    
    _doorMarker1.Close();
    _doorMarker2.Close();
  }
}
