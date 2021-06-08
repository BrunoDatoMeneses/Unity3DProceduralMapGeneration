using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Material matMaterial;

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public GameObject meshesParent;

    public GameObject hugeMapParent;

    public MeshFilter[] bigMapMeshFilters;
    public MeshRenderer[] bigMapMeshRenderers;

    public GameObject[,] hugeMapMeshes;

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

    public void DrawMHugeMap(MeshData meshData, Texture2D texture, int indice, Vector2 position, int nbChunkBySize)
    {
        if(hugeMapMeshes==null)
        {
            hugeMapMeshes = new GameObject[nbChunkBySize, nbChunkBySize];
            for (int i = 0; i < nbChunkBySize; i++)
            {
                for (int j = 0; j < nbChunkBySize; j++)
                {
                    hugeMapMeshes[i, j] = new GameObject();
                    hugeMapMeshes[i, j].name = "" + i + j;
                    hugeMapMeshes[i, j].AddComponent<Transform>();
                    hugeMapMeshes[i, j].transform.parent = hugeMapParent.transform;
                    hugeMapMeshes[i, j].AddComponent<MeshFilter>();
                    hugeMapMeshes[i, j].AddComponent<MeshRenderer>();
                    hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material = matMaterial;
                    hugeMapMeshes[i, j].transform.localScale = new Vector3(10.0f, 10.0f, 10.0f); 
                }
                

            }
        }

        for (int i = 0; i < nbChunkBySize; i++)
        {
            for (int j = 0; j < nbChunkBySize; j++)
            {
                hugeMapMeshes[i, j].GetComponent<MeshFilter>().mesh = meshData.CreateMesh();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material.mainTexture = texture;
                Vector3 meshPosition = new Vector3(10*i*MapGenerator.mapChunkSize-1, 0.0f, 10*j * MapGenerator.mapChunkSize-1);
                hugeMapMeshes[i, j].transform.position = meshPosition; 
            }


        }




        /*bigMapMeshFilters[indice].sharedMesh = meshData.CreateMesh();
        bigMapMeshRenderers[indice].material.mainTexture = texture;
        bigMapMeshFilters[indice].transform.position = new Vector3(position.x, 0, position.y);
        bigMapMeshRenderers[indice].transform.position = new Vector3(position.x, 0, position.y);*/




    }
}
