using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public GameObject meshesParent;


    public MeshFilter[] bigMapMeshFilters;
    public MeshRenderer[] bigMapMeshRenderers;

    public void DrawTexture(Texture2D texture)
    {

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void DrawMeshByIndice(MeshData meshData, Texture2D texture, int indice, Vector2 position)
    {
        bigMapMeshFilters[indice].sharedMesh = meshData.CreateMesh();
        bigMapMeshRenderers[indice].material.mainTexture = texture;
        bigMapMeshFilters[indice].transform.position = new Vector3(position.x, 0, position.y);
        bigMapMeshRenderers[indice].transform.position = new Vector3(position.x, 0, position.y);




    }
}
