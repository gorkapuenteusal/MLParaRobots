using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EdgeGenerator
{
  public static float[,] GenerateEdgeMap(int size) {
    float [,] map = new float[size, size];

    for (int i = 0; i < size; i++)
    {
        for (int j = 0; j < size; j++)
        {
            float x = i / (float) size * 2 - 1;
            float y = j / (float) size * 2 - 1;
      
            float value = Mathf.Clamp01(Mathf.Pow(x * x + y * y, 1 / 4f));

            map[i, j] = value;
        }
    }

    return map;
  }
}
