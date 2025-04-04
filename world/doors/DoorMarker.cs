using Flamme.common.enums;
using Flamme.common.helpers;
using Godot;
using System;

namespace Flamme.world.doors;

[Tool]
public partial class DoorMarker : StaticBody2D
{
  [Export] public Cardinal FacingDirection
  {
    get => _facingDirection;
    set
    {
      RotationDegrees = value switch
      {
        Cardinal.North => 0,
        Cardinal.East => 90,
        Cardinal.South => 180,
        Cardinal.West => -90,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
      };
      _facingDirection = value;
    }
  }
  private Cardinal _facingDirection = Cardinal.North;
  
  [ExportGroup("Textures")]
  [Export] public Texture2D DisguiseTexture;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Area2D TeleportZone;
  [Export] public Node2D TeleportPoint;

  [Signal] public delegate void TeleportEventHandler(entities.player.PlayableCharacter character);
  
  public Door Door { get; set; }

  public Texture2D TextureOpen = null;
  public Texture2D TextureClosed = null;
  
  public override void _Ready()
  {
    if(Engine.IsEditorHint())
      return;
    
    ExportMetaNonNull.Check(this);

    TeleportZone.BodyEntered += TeleportZoneOnBodyEntered;
  }

  private void TeleportZoneOnBodyEntered(Node2D body)
  {
    if (body is entities.player.PlayableCharacter playableCharacter)
    {
      EmitSignal(SignalName.Teleport, playableCharacter);
    }
  }

  // This class has no concept of states, just does as told
  public void Open()
  {
    Sprite.Texture = TextureOpen;
    SetDeferred(CollisionObject2D.PropertyName.CollisionLayer, (int)CollisionFlags.Entities);
  }

  public void Close()
  {
    Sprite.Texture = TextureClosed;
    SetDeferred(CollisionObject2D.PropertyName.CollisionLayer, (int)(CollisionFlags.Entities | CollisionFlags.Walls));
  }

  public void Disguise()
  {
    Sprite.Texture = DisguiseTexture;
    SetDeferred(CollisionObject2D.PropertyName.CollisionLayer, (int)(CollisionFlags.Entities | CollisionFlags.Walls));
  }
}