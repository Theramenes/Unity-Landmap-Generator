using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TextureGenerator
{
    /*
     * 生成地形颜色图
     */
    public static Color[] CreateTerrianMapColor(float[,] heightMap, TerrainType[] terrains)
    {
        int size = heightMap.GetLength(0);

        Color[] colorMap = new Color[size * size];
        for (int y = 0; y < size; y++) 
        {
            for (int x = 0; x < size; x++)
            {
                float currentHeight = heightMap[x, y];
                for (int i = 0; i < terrains.Length; i++)
                {
                    if (currentHeight <= terrains[i].height)
                    {
                        colorMap[y * size + x] = terrains[i].color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }

    /*
     * 生成黑白perlin noise颜色图
     */
    public static Color[] CreateNoiseMapColor(float[,] noiseMap)
    {
        int size = noiseMap.GetLength(0);

        Color[] colorMap = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                colorMap[y * size + x] = Color.Lerp(Color.white, Color.black, noiseMap[x, y]);
            }
        }

        return colorMap;
    }

    /*
     * param: chunk size 和 材质对应颜色图
     */
    public static Texture2D CreateMapTexture(Color[] colorMap)
    {
        int size = MapGenerator.mapChunkSize;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}
