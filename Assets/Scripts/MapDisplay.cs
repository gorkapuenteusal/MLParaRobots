// Script from the Sebastian Lague series on Procedural Terrain Generation https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
  public Renderer textureRenderer;

  public void DrawTexture(Texture2D texture) { 
    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
  }
}
