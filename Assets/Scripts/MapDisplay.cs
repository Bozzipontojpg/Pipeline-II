using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public void DrawTexture(Texture2D texture, Material mat)
    {
        textureRender.material = mat;
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);

        textureRender.gameObject.SetActive(true);
        MeshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture, Material mat)
    {
        MeshRenderer.material = mat;

        MeshFilter.sharedMesh = meshData.CreateMesh();
        MeshRenderer.sharedMaterial.mainTexture = texture;

        textureRender.gameObject.SetActive(false);
        MeshFilter.gameObject.SetActive(true);
    }
    public void DrawMeshTexture(MeshData meshData, Material mat)
    {
        textureRender.material = mat;
        MeshRenderer.material = mat;

        MeshFilter.sharedMesh = meshData.CreateMesh();
        textureRender.gameObject.SetActive(false);
        MeshFilter.gameObject.SetActive(true);
    }
}
