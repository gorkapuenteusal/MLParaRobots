// Script from the Sebastian Lague series on Procedural Terrain Generation https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
  public int width;
  public int height;
  public float scale;

  public bool autoUpdate;

  public void GenerateMap() {
    float[,] noiseMap = Noise.GenerateNoiseMap(width, height, scale);

    MapDisplay display = FindObjectOfType<MapDisplay>();
    display.DrawNoiseMap(noiseMap);
  }
}
