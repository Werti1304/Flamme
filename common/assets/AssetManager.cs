#region *.hpp
// //----------------------------------------------------------------------------------------------------------------------
// // AssetManager.cs
// //
// // <Explanation of the file's contents ...>
// // <... may span multiple lines.>
// //
// // Author: <name>, <name> ...
// //----------------------------------------------------------------------------------------------------------------------
// //
#endregion

using Godot;
using Godot.Collections;
using System.IO;

namespace Flamme.common.assets;

public partial class AssetManager : Node
{
  private static readonly Vector2I SpriteSize = new Vector2I(32, 32);
  
  public enum Asset
  {
    SpriteItemStatup1
  }

  public readonly Dictionary<Asset, Texture2D> Textures = new Dictionary<Asset, Texture2D>();

  public AssetManager()
  {
    _instance = this;

    Textures[Asset.SpriteItemStatup1] = GD.Load<Texture2D>(Path.Combine(AssetPaths.AssetPath, AssetPaths.StatupPath1));
  }

  public AtlasTexture GetAtlasTexture(Asset asset, Vector2I atlasCoords)
  {
    atlasCoords = atlasCoords * 32;
    var atlas = Textures[asset];
    var atlasTexture = new AtlasTexture();
    atlasTexture.SetAtlas(atlas);
    atlasTexture.SetRegion(new Rect2(atlasCoords, SpriteSize));
    return atlasTexture;
  }

public static AssetManager Instance
  {
    get
    {
      lock (Padlock)
      {
        return _instance;
      }
    }
  }
  
  private static AssetManager _instance;
  private static readonly object Padlock = new object();
}
