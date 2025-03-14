using Flamme.common.enums;
using Flamme.testing;
using Godot;

namespace Flamme.world.doors;

public partial class DoorMarker : StaticBody2D
{
  [ExportGroup("Textures")]
  [Export] public Texture2D DisguiseTexture;
  
  [ExportGroup("Meta")]
  [Export] public Sprite2D Sprite;
  [Export] public CollisionShape2D CollisionShape;
  [Export] public Area2D TeleportZone;
  [Export] public Node2D TeleportPoint;
  
  [Signal] public delegate void TeleportEventHandler(PlayableCharacter character);
  
  public Door Door { get; set; }
  
  public Cardinal FacingDirection = Cardinal.South;

  public Texture2D TextureOpen = null;
  public Texture2D TextureClosed = null;
  
  public override void _Ready()
  {
    ExportMetaNonNull.Check(this);

    TeleportZone.BodyEntered += TeleportZoneOnBodyEntered;
  }

  private void TeleportZoneOnBodyEntered(Node2D body)
  {
    if (body is PlayableCharacter playableCharacter)
    {
      EmitSignal(SignalName.Teleport, playableCharacter);
    }
  }

  public void SetFacingDirection(Cardinal cardinal)
  {
    RotationDegrees = FacingDirection.GetRotationTo(cardinal);
    FacingDirection = cardinal;
  }

  // This class has no concept of states, just does as told
  public void Open()
  {
    Sprite.Texture = TextureOpen;
    CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
  }

  public void Close()
  {
    Sprite.Texture = TextureClosed;
    CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
  }

  public void Disguise()
  {
    Sprite.Texture = DisguiseTexture;
    CollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
  }
}