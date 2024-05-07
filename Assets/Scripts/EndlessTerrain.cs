using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
  const float scale = 3f;
  const float viewerMoveThreshold4ChunkUpdate = 25f;
  const float sqrtViewerMoveThreshold4ChunkUpdate = viewerMoveThreshold4ChunkUpdate * viewerMoveThreshold4ChunkUpdate;

  public static float maxViewDistance;
  public LODInfo[] detailLevels;

  public Transform viewer;
  public Material mapMaterial;

  public static Vector2 viewerPosition;
  Vector2 viewerPositionOld;
  static MapGenerator mapGenerator;

  int chunkSize;
  int visibleChunksInViewDst;

  Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
  static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

  void Start() {
    chunkSize = MapGenerator.mapChunkSize - 1;
    maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    visibleChunksInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);
    mapGenerator = FindObjectOfType<MapGenerator>();

    UpdateVisibleChunks();
  }

  void Update() {
    viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

    if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveThreshold4ChunkUpdate) {
      viewerPositionOld = viewerPosition;
      UpdateVisibleChunks();
    }
  }

  void UpdateVisibleChunks() {
    for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
    {
      terrainChunksVisibleLastUpdate[i].SetVisible(false);
    }

    terrainChunksVisibleLastUpdate.Clear();

    int currentChunkCoordsX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
    int currentChunkCoordsY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
    for (int yOffset = -visibleChunksInViewDst; yOffset <= visibleChunksInViewDst; yOffset++)
    {    
      for (int xOffset = -visibleChunksInViewDst; xOffset <= visibleChunksInViewDst; xOffset++)
      {
        Vector2 viewChunkCoords = new Vector2(currentChunkCoordsX + xOffset, currentChunkCoordsY + yOffset);

        if (terrainChunkDict.ContainsKey(viewChunkCoords)) {
          terrainChunkDict[viewChunkCoords].UpdateTerrainChunk();
          
        } else {
          terrainChunkDict.Add(viewChunkCoords, new TerrainChunk(viewChunkCoords, chunkSize, detailLevels, transform, mapMaterial));
        }     
      }
    }
  }

  public class TerrainChunk {
    GameObject meshObject;
    Vector2 pos;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;

    MapData mapData;
    bool mapDataReceived;

    int prevLODIdx = -1;

    public TerrainChunk(Vector2 coords, int size, LODInfo[] detailLevels, Transform parent, Material material) {
      this.detailLevels = detailLevels;

      pos = coords * size;
      Vector3 pos3d = new Vector3(pos.x, 0, pos.y);
      bounds = new Bounds(pos, Vector2.one * size);

      meshObject = new GameObject("Terrain Chunk");
      meshRenderer = meshObject.AddComponent<MeshRenderer>();
      meshFilter = meshObject.AddComponent<MeshFilter>();
      meshRenderer.material = material;

      meshObject.transform.position = pos3d * scale;
      meshObject.transform.parent = parent;
      meshObject.transform.localScale = Vector3.one * scale;
      SetVisible(false);

      lodMeshes = new LODMesh[detailLevels.Length];
      for (int i = 0; i < detailLevels.Length; i++)
      {
          lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
      }

      mapGenerator.RequestMapData(pos, OnMapDataReceived);
    }

    void OnMapDataReceived(MapData mapData) {
      this.mapData = mapData;
      mapDataReceived = true;

      Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
      meshRenderer.material.mainTexture = texture;

      UpdateTerrainChunk();
    }

    void OnMeshDataReceived(MeshData meshData) {
      meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk() {
      if (mapDataReceived) {
        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
        bool visible = viewerDstFromNearestEdge <= maxViewDistance;

        if (visible) {
          int lodIdx = 0;

          for (int i = 0; i < detailLevels.Length - 1; i++)
          {
              if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                lodIdx = i + 1;
              } else {
                break;
              }
          }

          if (lodIdx != prevLODIdx) {
            LODMesh lodMesh = lodMeshes[lodIdx];
            if (lodMesh.hasMesh) {
              prevLODIdx = lodIdx;
              meshFilter.mesh = lodMesh.mesh;
            } else if (!lodMesh.hasRequestedMesh) {
              lodMesh.RequestMesh(mapData);
            }
          }

          terrainChunksVisibleLastUpdate.Add(this);
        }

        SetVisible(visible);
      }
    }

    public void SetVisible(bool visible) {
      meshObject.SetActive(visible);
    }

    public bool IsVisible() {
      return meshObject.activeSelf;
    }
  }

  class LODMesh {
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    System.Action updateCallback;

    public LODMesh(int lod, System.Action updateCallback) {
      this.lod = lod;
      this.updateCallback = updateCallback;
    }

    void OnMeshDataReceived(MeshData meshData) {
      mesh = meshData.CreateMesh();
      hasMesh = true;

      updateCallback();
    }

    public void RequestMesh(MapData mapData) {
      hasRequestedMesh = true;
      mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
    }
  }

  [System.Serializable]
  public struct LODInfo {
    public int lod;
    public float visibleDstThreshold;
  }
}
