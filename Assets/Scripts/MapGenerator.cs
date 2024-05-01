// Script from the Sebastian Lague series on Procedural Terrain Generation https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
  public int width;
  public int height;
  public float scale;

  // Each octave is basically a finer (higher frequency) version of noise added to the previous accumulated noise. 
  public int octaves;
  // The smaller it is, the larger terrain features are more dominant, and smaller ones (fine details) are less noticeable.
  [Range(0, 1)]  
  public float persistance;
  // It affects the "density" or "compaction" of noise characteristics in space, allowing the terrain to have more variations at smaller scales.
  public float lacunarity;

  public int seed;
  public Vector2 offset; 

  public bool autoUpdate;

  public void GenerateMap() {
    float[,] noiseMap = Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);

    MapDisplay display = FindObjectOfType<MapDisplay>();
    display.DrawNoiseMap(noiseMap);
  }

  void OnValidate() {
    if (width < 1) {
      width = 1;
    }
    if (height < 1) {
      height = 1;
    }
    if (lacunarity < 1) {
      lacunarity = 1;
    }
    if (octaves < 0) {
      octaves = 0;
    }
    if (scale < 0.001f) {
      scale = 0.001f;
    }
  }
}
