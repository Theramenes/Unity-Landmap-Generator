using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/*
 * 调用PerlinNoise和MapDisplay
 */
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, TerrianMap, Mesh, FalloffMap};

    [Header("Choosing Draw Mode and Normalize Mode")]
    public DrawMode drawMode;
    public PerlinNoise.NormalizeMode normalizeMode;

    [Header("Map Size")]
    public const int mapChunkSize = 121;
    [Range(0,6)]
    public int LODPreview ;
    public float mapScale = 1;

    [Header("Generator Parameters")]
    public float noiseScale;
    [Range(0,20)]
    public int octaves; 
    [Range(0,1)]
    public float persistence;
    public float lacunarity;
    public float meshHeightAmplitude;
    public AnimationCurve meshHeightCurveEditor;

    [Header("Generator Seed")]
    public int seed;
    public Vector2 offset;

    [Space(10)]
    public bool autoUpdate;
    public bool activateFalloff;

    [Header("Terrians")]
    public TerrainType[] terrains;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    float[,] falloffMap;

    private void Awake()
    {
        falloffMap = FalloffMapGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public MapData GenerateMapData(Vector2 mapCenter)
    {
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapChunkSize,
                                                         mapChunkSize,
                                                         noiseScale,
                                                         seed,
                                                         octaves,
                                                         persistence,
                                                         lacunarity,
                                                         mapCenter + offset,
                                                         normalizeMode);

        Color[] colorMapNoise = new Color[mapChunkSize * mapChunkSize];
        Color[] colorMapTerrian = new Color[mapChunkSize * mapChunkSize];

        colorMapNoise = TextureGenerator.CreateNoiseMapColor(noiseMap);
        colorMapTerrian = TextureGenerator.CreateTerrianMapColor(noiseMap, terrains);
        
        return new MapData(noiseMap, colorMapNoise, colorMapTerrian);
    }

    public void DrawPreviewMapInInspect()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.TerrianMap)
            mapDisplay.DrawTexture(TextureGenerator.CreateMapTexture(mapData.colorMapTerrian));
        else if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(TextureGenerator.CreateMapTexture(mapData.colorMapNoise));
        else if (drawMode == DrawMode.Mesh)
            mapDisplay.DrawMesh(
                MeshGenerator.GenerateTerrianMesh(mapData.heightMap, meshHeightAmplitude, LODPreview, meshHeightCurveEditor),
                TextureGenerator.CreateMapTexture(mapData.colorMapTerrian),
                mapScale);
        else if (drawMode == DrawMode.FalloffMap)
            mapDisplay.DrawTexture(TextureGenerator.CreateMapTexture(TextureGenerator.CreateNoiseMapColor(FalloffMapGenerator.GenerateFalloffMap(mapChunkSize))));
            
    }

    /* 
     * Threading
     * 多线程生成mapData和MeshData
     * 通过回调的方式 传入Data 
     *
     */

    /* description: 开始新线程
     * param: 一个委托类 接收MapData类型参数的Action
     */
    public void RequestMapData(Vector2 mapCenter, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(mapCenter, callback); };
        new Thread(threadStart).Start();
    }

    /* description: 线程执行的方法， 调用GenerateMapData 
     * param: callback -> 委托类 同得到的mapData 一起放入threadInfo 
     *                    压入队列等待Updata取出调用
     */
    private void MapDataThread(Vector2 mapCenter, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(mapCenter);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    /*
     * 
     */
    public void RequestMeshData(MapData mapData,int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod,  callback); };
        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData,int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrianMesh(mapData.heightMap, meshHeightAmplitude, lod, meshHeightCurveEditor);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }


    /* description: 每次更新时将mapDataThreadInfoQueue 和 meshDataThreadInfoQueue 
     *              中的 ThreadInfo 取出， 执行callback 方法
     */
    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                // callback 为 OnMapDataRecieve 
                MapThreadInfo<MapData> mapThreadInfo = mapDataThreadInfoQueue.Dequeue();
                mapThreadInfo.callback(mapThreadInfo.parameter);
                
            }
        }

        while (meshDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<MeshData> meshThreadInfo = meshDataThreadInfoQueue.Dequeue();
            meshThreadInfo.callback(meshThreadInfo.parameter);
        }
    }

    private void OnValidate()
    {
        if (lacunarity < 1)
            lacunarity = 1;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMapNoise;
    public readonly Color[] colorMapTerrian;

    public MapData(float[,] heightMap, Color[] colorMapNoise, Color[] colorMapTerrian)
    {
        this.heightMap = heightMap;
        this.colorMapNoise = colorMapNoise;
        this.colorMapTerrian = colorMapTerrian;
    }
}