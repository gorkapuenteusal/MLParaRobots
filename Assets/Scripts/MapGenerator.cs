// Script from the Sebastian Lague series on Procedural Terrain Generation https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
  public enum DrawMode {NoiseMap, ColorMap, Mesh, EdgeMap};
  public DrawMode drawMode;

  public Noise.NormalizeMode normalizeMode;

  public const int mapChunkSize = 241;
  [Range(0,6)]
  public int editorPreviewLOD;
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

  public float meshHeightMultiplier;
  public AnimationCurve meshHeightCurve;

  public bool autoUpdate;

  public TerrainTypes[] regions;

  Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
  Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

  float[,] edgeMap;

  void Awake() {
    edgeMap = EdgeGenerator.GenerateEdgeMap(mapChunkSize);
  }

  public void DrawMapInEditor() {
    MapData mapData = GenerateMapData(Vector2.zero);
    MapDisplay display = FindObjectOfType<MapDisplay>();
    if (drawMode == DrawMode.NoiseMap)
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
    else if (drawMode == DrawMode.ColorMap)
      display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
    else if (drawMode == DrawMode.Mesh)
      display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
    else if (drawMode == DrawMode.EdgeMap)
      display.DrawTexture(TextureGenerator.TextureFromHeightMap(EdgeGenerator.GenerateEdgeMap(mapChunkSize)));
  }

  public void RequestMapData(Vector2 center, Action<MapData> callback) {
    ThreadStart threadStart = delegate {
      MapDataThread(center, callback);
    };

    new Thread(threadStart).Start();
  }

  public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback) {
    ThreadStart threadStart = delegate {
      MeshDataThread(mapData, lod, callback);
    };

    new Thread(threadStart).Start();
  }

  void MapDataThread(Vector2 center, Action<MapData> callback) {
    MapData mapData = GenerateMapData(center);
    lock (mapDataThreadInfoQueue) {
      mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
    }
  }

  void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback) {
    MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
    lock (meshDataThreadInfoQueue) {
      meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
    }
  }

  void Update() {
    if (mapDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
      {
          MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
          threadInfo.callback(threadInfo.parameter);
      }
    }

    if (meshDataThreadInfoQueue.Count > 0) {
      for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
      {
          MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
          threadInfo.callback(threadInfo.parameter);
      }
    }
  }

  MapData GenerateMapData(Vector2 center) {
    float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, scale, octaves, persistance, lacunarity, center + offset, normalizeMode);
    Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
    for (int y = 0; y < mapChunkSize; y++)
    {
        for (int x = 0; x < mapChunkSize; x++)
        {
            noiseMap[x, y] = Mathf.Clamp01(edgeMap[x, y] - noiseMap[x, y]);
            float currentHeight = noiseMap[x, y];
            for (int i = 0; i < regions.Length; i++)
            {
                if (currentHeight >= regions[i].height) {
                  colorMap[y * mapChunkSize + x] = regions[i].color;
                } else {
                  break;
                }
            }
        }
    }
    return new MapData(noiseMap, colorMap);
  }

  void OnValidate() {
    if (lacunarity < 1) {
      lacunarity = 1;
    }
    if (octaves < 0) {
      octaves = 0;
    }

    edgeMap = EdgeGenerator.GenerateEdgeMap(mapChunkSize);
  }

  struct MapThreadInfo<T> {
    public readonly Action<T> callback;
    public readonly T parameter;

    public MapThreadInfo(Action<T> callback, T parameter) {
      this.callback = callback;
      this.parameter = parameter;
    }
  }
}

[System.Serializable]
public struct TerrainTypes {
  public string name;
  public float height;
  public Color color;
}

public struct MapData {
  public readonly float[,] heightMap;
  public readonly Color[] colorMap;

  public MapData (float[,] heightMap, Color[] colorMap) {
    this.heightMap = heightMap;
    this.colorMap = colorMap;
  }
}
