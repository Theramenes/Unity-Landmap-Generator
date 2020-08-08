using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class InfiniteMapGeneration : MonoBehaviour
{
    [Header("View Distance Config")]
    public static float viewDistance;
    public LODInfo[] lodLevels;

    [Header("Infinite Map Generation Parameters")]
    public float mapScale = 1f;

    [Header("LOD options")]
    [Range(1, 4)]
    public int collisionMeshLODLevel = 2;

    [Space(10)]
    public Transform viewer;
    // viewerPosition : 观察位置
    public static Vector2 viewerPosition;
    private Vector2 viewerPositionLastUpdate;
    public Material material;


    // chunksVisible : 从当前位置到最大视距之间 可见的chunks
    private int chunkSize;
    private int chunksVisible;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> terrainChunksVisibleList = new List<TerrainChunk>();
    
    private static MapGenerator mapGenerator;

    private float viewerUpdateThreshold = 40f;
    private float sqrViewerUpdateThreshold;

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        sqrViewerUpdateThreshold = viewerUpdateThreshold * viewerUpdateThreshold;
        viewDistance = lodLevels[lodLevels.Length - 1].lodThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisible = Mathf.RoundToInt(viewDistance / chunkSize);

        UpdateChunksInViewDistance();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapScale;
        if((viewerPositionLastUpdate - viewerPosition).sqrMagnitude > sqrViewerUpdateThreshold)
        {
            UpdateChunksInViewDistance();
            viewerPositionLastUpdate = viewerPosition;
        }
    }

    void UpdateChunksInViewDistance()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        foreach(TerrainChunk terrainChunk in terrainChunksVisibleList)
        {
            terrainChunk.SetChunkVisible(false);
        }
        terrainChunksVisibleList.Clear();


        for(int yOffset = -chunksVisible; yOffset < chunksVisible; yOffset++)
        {
            for(int xOffset = -chunksVisible; xOffset < chunksVisible; xOffset++)
            {
                Vector2 visibleChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                TerrainChunk terrainChunk;
                if (terrainChunkDictionary.TryGetValue(visibleChunkCoord,out terrainChunk))
                {
                    terrainChunkDictionary[visibleChunkCoord].UpdateChunkVisible();
                }
                else
                {
                    terrainChunk = new TerrainChunk(visibleChunkCoord, chunkSize, mapScale, lodLevels, transform, material, collisionMeshLODLevel);
                    terrainChunkDictionary.Add(visibleChunkCoord, terrainChunk);
                }
                    terrainChunksVisibleList.Add(terrainChunk);
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 chunkPosition;
        Bounds bounds;

        GameObject meshObject;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] lodLevels;
        LODMesh[] lodMeshs;
        int currentLODLevel = -1;

        MapData mapData;
        bool isMapDataRecieved = false;

        LODMesh collisionLODMesh;

        public TerrainChunk(Vector2 coord,
                            int size,
                            float scale,
                            LODInfo[] lodLevels,
                            Transform parent,
                            Material material,
                            int collisionMeshLODLovel)
        {
            this.lodLevels = lodLevels;

            chunkPosition = coord * size;
            Vector3 chunkPositionV3 = new Vector3(chunkPosition.x, 0, chunkPosition.y);
            bounds = new Bounds(chunkPosition, Vector2.one * size);

            // 创建 mesh
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;
            // 设置 mesh position transform
            meshObject.transform.position = chunkPositionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            SetChunkVisible(false);

            lodMeshs = new LODMesh[lodLevels.Length];
            for (int i = 0; i < lodLevels.Length; i++)
            {
                lodMeshs[i] = new LODMesh(lodLevels[i].lod, UpdateChunkVisible);
                if (i == collisionMeshLODLovel)
                    collisionLODMesh = lodMeshs[i];
            }

            mapGenerator.RequestMapData(chunkPosition, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            isMapDataRecieved = true;

            meshRenderer.material.mainTexture = TextureGenerator.CreateMapTexture(mapData.colorMapTerrian);

            UpdateChunkVisible();
        }

        public void UpdateChunkVisible()
        {
            if(isMapDataRecieved)
            {
                float viewerDistFromNearestBound = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistFromNearestBound <= viewDistance;

                int updateLODLevel = 0;
                if (visible)
                {
                    for(int i = 0; i < lodLevels.Length - 1; i++) // 距离不可能超过lod 最大距离
                    {
                        if (viewerDistFromNearestBound > lodLevels[i].lodThreshold)
                            updateLODLevel++;
                        else
                            break;
                    }

                    if(updateLODLevel != currentLODLevel)
                    {
                        LODMesh lodMesh = lodMeshs[updateLODLevel];
                        if (lodMesh.hasMesh)
                        {
                            meshFilter.mesh = lodMesh.mesh;
                            currentLODLevel = updateLODLevel;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                        if (!lodMesh.hasRequestMesh)
                            lodMesh.RequestMeshData(mapData);
                    }

                    if(updateLODLevel == lodLevels[0].lod)
                    {
                        if (collisionLODMesh.hasMesh)
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        else
                            collisionLODMesh.RequestMeshData(mapData);
                    }

                    terrainChunksVisibleList.Add(this);
                }

                SetChunkVisible(visible);
            }
        }

        public void SetChunkVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsChunkVisible()
        {
            return meshObject.activeSelf;
        }

    }

    class LODMesh
    {
        public Mesh mesh;
        private int lod;

        public bool hasMesh = false;
        public bool hasRequestMesh = false;

        Action updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        } 

        public void RequestMeshData(MapData mapData)
        {
            hasRequestMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }

        void OnMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float lodThreshold;
    }
}
