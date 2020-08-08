using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffMapGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] falloffMap = new float[size, size];

        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                float a = x / (float)size * 2 - 1;
                float b = y / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));
                falloffMap[x, y] = value;
            }
        }

        return falloffMap;
    }
}
