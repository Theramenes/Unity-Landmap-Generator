using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * PerlinNoise生成工具 
 */

public static class PerlinNoise
{
    public enum NormalizeMode { Local, Global};

    /*
     * Input: 地图宽，高，scale
     * perlinNoise 生成的是伪随机，通过scale生成不同的结果
     */
    public static float[,] GenerateNoiseMap(int mapWidth,
                                            int mapHeight,
                                            float scale,
                                            int seed,
                                            int octaves,
                                            float persistance,
                                            float lacunarity,
                                            Vector2 offset,
                                            NormalizeMode normalizeMode)
    {
        float frequency = 1;
        float amplitude = 1;
        float noiseHeight = 0;

        float[,] perlinNoiseMap = new float[mapWidth, mapHeight];

        System.Random pseudoRandomNumber = new System.Random(seed);
        Vector2[] octaveOffsets = GenerateOffsetsOnSeeds(octaves, seed, offset);

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        float maxGlobalHeight = GetGlobalMaxHeight(octaves, amplitude, persistance);

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                frequency = 1;
                amplitude = 1;
                noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = ((x - mapWidth / 2f) + octaveOffsets[i].x) / scale * frequency;
                    float yCoord = ((y - mapHeight / 2f) + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                maxLocalNoiseHeight = maxLocalNoiseHeight > noiseHeight ? maxLocalNoiseHeight : noiseHeight;
                minLocalNoiseHeight = minLocalNoiseHeight < noiseHeight ? minLocalNoiseHeight : noiseHeight;
                perlinNoiseMap[x, y] = noiseHeight;
            }  

        }

        // normalization 
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                if(normalizeMode == NormalizeMode.Local)
                    perlinNoiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, perlinNoiseMap[x, y]);
                else
                {
                    float nomalizedHeight = (perlinNoiseMap[x, y] + 1) / (2f * maxGlobalHeight / 2f);
                    perlinNoiseMap[x, y] = Mathf.Clamp01(nomalizedHeight);
                }

            }
        }
        return perlinNoiseMap;
    }

    /*
     * 根据seed随机生成 offset
     * 将offset 添加到Coord中
     */
    private static Vector2[] GenerateOffsetsOnSeeds(int octaves, int seed, Vector2 offset)
    {
        System.Random pseudoRandomNumber = new System.Random(seed);

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float xOffset = pseudoRandomNumber.Next(-100000, 100000) + offset.x;
            float yOffset = pseudoRandomNumber.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(xOffset, yOffset);
        }

        return octaveOffsets;
    }

    private static float GetGlobalMaxHeight(int octaves, float amplitude, float persistance)
    {
        float maxGlobalHeight = 0;
        while (octaves > 0)
        {
            maxGlobalHeight += amplitude;
            amplitude *= persistance;
            octaves--;
        }
        return maxGlobalHeight;
    }

}
