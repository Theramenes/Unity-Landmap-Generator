using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 映射PerlinNoise到黑白色
 * 更新对应材质
 */
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    // 
    public void DrawTexture(Texture2D texture)
    {

        // 用sharedMaterial 不进入游戏即可更新材质
        textureRenderer.sharedMaterial.mainTexture = texture;
        // 随PerlinNoise更改平面大小
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture, float scale)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.transform.localScale = Vector3.one * scale;
    }


}
