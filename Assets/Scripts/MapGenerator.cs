// Script from the Sebastian Lague series on Procedural Terrain Generation https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
  public enum DrawMode {NoiseMap, ColorMap};
  public DrawMode drawMode;

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

  public TerrainTypes[] regions;

  public void GenerateMap() {
    float[,] noiseMap = Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);
    Color[] colorMap = new Color[width * height];
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            float currentHeight = noiseMap[x, y];
            for (int i = 0; i < regions.Length; i++)
            {
                if (currentHeight <= regions[i].height) {
                  colorMap[y * width + x] = regions[i].color;
                  break;
                }
            }
        }
    }

    MapDisplay display = FindObjectOfType<MapDisplay>();
    if (drawMode == DrawMode.NoiseMap)
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
    else if (drawMode == DrawMode.ColorMap)
      display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, width, height));

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

[System.Serializable]
public struct TerrainTypes {
  public string name;
  public float height;
  public Color color;
}
