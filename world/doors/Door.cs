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
    Secret
  }
  
  [Export] private DoorType _type = DoorType.Bars;
  [Export] public DoorMarker DoorMarker1;
  [Export] public DoorMarker DoorMarker2;
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
  [Export] public AtlasTexture SecretClosedTexture;
  [Export] public AtlasTexture SecretOpenTexture;
  
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
    if (DoorMarker1 == null || DoorMarker2 == null)
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

    DoorMarker1.Door = this;
    DoorMarker2.Door = this;
    
    switch (_type)
    {
      case DoorType.Bars:
        DoorMarker1.TextureOpen = BarOpenTexture;        
        DoorMarker2.TextureOpen = BarOpenTexture;
        DoorMarker1.TextureClosed = BarClosedTexture;
        DoorMarker2.TextureClosed = BarClosedTexture;
        break;
      case DoorType.Boss:
        DoorMarker1.TextureOpen = BossOpenTexture;        
        DoorMarker2.TextureOpen = BossOpenTexture;
        DoorMarker1.TextureClosed = BossClosedTexture;
        DoorMarker2.TextureClosed = BossClosedTexture;
        break;
      case DoorType.Gold:
        DoorMarker1.TextureOpen = GoldOpenTexture;        
        DoorMarker2.TextureOpen = GoldOpenTexture;
        DoorMarker1.TextureClosed = GoldClosedTexture;
        DoorMarker2.TextureClosed = GoldClosedTexture;
        _isLockedByKey = true;
        break;
      case DoorType.Shop:
        DoorMarker1.TextureOpen = ShopOpenTexture;        
        DoorMarker2.TextureOpen = ShopOpenTexture;
        DoorMarker1.TextureClosed = ShopClosedTexture;
        DoorMarker2.TextureClosed = ShopClosedTexture;
        _isLockedByKey = true;
        break;
      case DoorType.Secret:
        DoorMarker1.TextureOpen = SecretOpenTexture;
        DoorMarker2.TextureOpen = SecretOpenTexture;
        DoorMarker1.TextureClosed = SecretClosedTexture;
        DoorMarker2.TextureClosed = SecretClosedTexture;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
    
    Open();
    
    DoorMarker1.Teleport += OnTeleportDoorMarker1;
    DoorMarker2.Teleport += OnTeleportDoorMarker2;
  }
  
  private void OnTeleportDoorMarker1(PlayableCharacter character)
  {
    Level.Current.Teleport(Room1, Room2, DoorMarker2.TeleportPoint.GlobalPosition);
  }
  
  private void OnTeleportDoorMarker2(PlayableCharacter character)
  {
    Level.Current.Teleport(Room2, Room1, DoorMarker1.TeleportPoint.GlobalPosition); 
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
    
    DoorMarker1.Open();
    DoorMarker2.Open();
  }

  public virtual void Close()
  {
    if (DebugToggles.DoorsAlwaysOpen)
      
    #pragma warning disable CS0162 // Unreachable code detected
    {
      Open();
      return;
    }
    
    _isOpen = false;
    
    DoorMarker1.Close();
    DoorMarker2.Close();
    
    #pragma warning restore CS0162 // Unreachable code detected
  }
}
