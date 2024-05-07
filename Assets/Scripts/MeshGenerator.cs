using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
  public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int lod) {
    AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

    int width = heightMap.GetLength(0);
    int height = heightMap.GetLength(1);
    float topLeftX = (width - 1) / -2f;
    float topLeftZ = (height - 1) / 2f;

    MeshData meshData = new MeshData(width, width);
    int vertexIdx = 0;

		int meshSimplificationIncrement = (lod == 0)?1:lod * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

    for (int y = 0; y < height; y += meshSimplificationIncrement)
    {
        for (int x = 0; x < width; x += meshSimplificationIncrement)
        {
          meshData.vertices[vertexIdx] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
          meshData.uvs[vertexIdx] = new Vector2(x / (float) width, y / (float) height);

          if (x < width - 1 && y < height - 1) {
            meshData.AddTriangle(vertexIdx, vertexIdx + verticesPerLine + 1, vertexIdx + verticesPerLine);
            meshData.AddTriangle(vertexIdx + verticesPerLine + 1, vertexIdx, vertexIdx + 1);
          }

          vertexIdx++;
        }
    }

    return meshData;
  }
}

public class MeshData {
  public Vector3[] vertices;
  public int[] triangles;
  public Vector2[] uvs;

  int triangleIdx;

  public MeshData(int meshWidth, int meshHeight) {
    vertices = new Vector3[meshWidth * meshHeight];
    uvs = new Vector2[meshWidth * meshHeight];
    triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
  }

  public void AddTriangle(int a, int b, int c) {
    triangles[triangleIdx] = a;
    triangles[triangleIdx + 1] = b;
    triangles[triangleIdx + 2] = c;

    triangleIdx += 3;
  }

  public Mesh CreateMesh() {
    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();
    return mesh;
  }
}
