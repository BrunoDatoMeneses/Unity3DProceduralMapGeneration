using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Material matMaterial;

    public Renderer singleNoiseMapTextureRenderer;
    public Renderer singleColourMapTextureRenderer;
    public Renderer fallOffMapTextureRenderer;
    public MeshFilter singleMeshFilter;
    public MeshRenderer singleMeshRenderer;
   

    public GameObject hugeMapParent;
    public Renderer hugeHeightMapTextureRenderer;
    public Renderer hugeColourMapTextureRenderer;
    public Renderer hugeFallOffMapTextureRenderer;
    public GameObject[,] hugeMapMeshes;

    public GameObject bigMapParent;
    public MeshFilter[] bigMapMeshFilters;
    public MeshRenderer[] bigMapMeshRenderers;

    public void DrawHeightTexture(Texture2D texture)
    {

        singleNoiseMapTextureRenderer.material.mainTexture = texture;
        singleNoiseMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawColourTexture(Texture2D texture)
    {

        singleColourMapTextureRenderer.material.mainTexture = texture;
        singleColourMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawFallOffTexture(Texture2D texture)
    {

        fallOffMapTextureRenderer.material.mainTexture = texture;
        fallOffMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        singleMeshFilter.mesh = meshData.CreateMesh();
        singleMeshRenderer.material.mainTexture = texture;
    }

    public void DrawMeshByIndice(MeshData meshData, Texture2D texture, int indice, Vector2 position)
    {
        bigMapMeshFilters[indice].mesh = meshData.CreateMesh();
        bigMapMeshRenderers[indice].material.mainTexture = texture;
        bigMapMeshFilters[indice].transform.position = new Vector3(position.x, 0, position.y);
        bigMapMeshRenderers[indice].transform.position = new Vector3(position.x, 0, position.y);




    }

    public void DrawHugeHeightTexture(Texture2D texture)
    {

        hugeHeightMapTextureRenderer.material.mainTexture = texture;
        hugeHeightMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawHugeColourTexture(Texture2D texture)
    {

        hugeColourMapTextureRenderer.material.mainTexture = texture;
        hugeColourMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawHugeFallOffTexture(Texture2D texture)
    {

        hugeFallOffMapTextureRenderer.material.mainTexture = texture;
        hugeFallOffMapTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawHugeMapMeshes(MapData mapData, int nbChunkBySize, int subSize, int hugeMapSize)
    {
        MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();

        if (hugeMapMeshes!=null)
        {
            for (int j = 0; j < hugeMapMeshes.GetLength(0); j++)
            {
                for (int i = 0; i < hugeMapMeshes.GetLength(1); i++)
                {
                    GameObject.Destroy(hugeMapMeshes[i, j]);
                }
            }
        }
        

        hugeMapMeshes = new GameObject[nbChunkBySize, nbChunkBySize];


        for (int j = 0; j < nbChunkBySize; j++)
        {
            for (int i = 0; i < nbChunkBySize; i++)
            {
                

                MapData subMapData = getSubMapData(mapData, subSize, hugeMapSize, i, j);

                MeshData meshData = MeshGenerator.GenerateTerrainMesh(subMapData.heightMap, mapGenerator.meshHeightMultiplier, mapGenerator.meshHeightCurve, mapGenerator.editorPreviewLOD);
                Texture2D meshTexture = TextureGenerator.TextureFromColourMap(subMapData.colourMap, subSize, subSize);

                hugeMapMeshes[i, j] = new GameObject();
                hugeMapMeshes[i, j].name = "" + i + j;
                //hugeMapMeshes[i, j].AddComponent<Transform>();
                hugeMapMeshes[i, j].transform.parent = hugeMapParent.transform;
                

                hugeMapMeshes[i, j].AddComponent<MeshFilter>();
                hugeMapMeshes[i, j].AddComponent<MeshRenderer>();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material = matMaterial;

                hugeMapMeshes[i, j].GetComponent<MeshFilter>().mesh = meshData.CreateMesh();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material.mainTexture = meshTexture;

                hugeMapMeshes[i, j].AddComponent<MeshCollider>();
                hugeMapMeshes[i, j].GetComponent<MeshCollider>().sharedMesh = hugeMapMeshes[i, j].GetComponent<MeshFilter>().mesh;

                float offset = (meshTexture.width-3) * 10.0f;
                hugeMapMeshes[i, j].transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
                hugeMapMeshes[i, j].transform.position = new Vector3(i * offset, 0.0f, -j * offset);
            }


        }

        




    }

    private static MapData getSubMapData(MapData mapData, int subSize, int hugeMapSize, int i, int j)
    {
        float[,] subHeighMap = new float[subSize, subSize];
        Color[] subColourMap = new Color[subSize * subSize];
        for (int y = 0; y < subSize; y++)
        {
            for (int x = 0; x < subSize; x++)
            {
                subHeighMap[x, y] = mapData.heightMap[(subSize*i) + x, (subSize * j) + y];
                subColourMap[y * subSize + x] = mapData.colourMap[(y+(subSize * j) ) * hugeMapSize + (x + (subSize * i))];

            }
        }

        MapData subMapData = new MapData(subHeighMap, subColourMap);
        return subMapData;
    }

    public void DrawHugeMapMeshesOld(MeshData meshData, Texture2D texture, int nbChunkBySize)
    {




        hugeMapMeshes = new GameObject[nbChunkBySize, nbChunkBySize];


        for (int i = 0; i < nbChunkBySize; i++)
        {
            for (int j = 0; j < nbChunkBySize; j++)
            {
                hugeMapMeshes[i, j] = new GameObject();
                hugeMapMeshes[i, j].name = "" + i + j;
                //hugeMapMeshes[i, j].AddComponent<Transform>();
                hugeMapMeshes[i, j].transform.parent = hugeMapParent.transform;
                hugeMapMeshes[i, j].AddComponent<MeshFilter>();
                hugeMapMeshes[i, j].AddComponent<MeshRenderer>();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material = matMaterial;
                
                hugeMapMeshes[i, j].GetComponent<MeshFilter>().mesh = meshData.CreateMesh();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material.mainTexture = texture;

                float offset = (texture.width - 1)*10.0f;
                hugeMapMeshes[i, j].transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
                hugeMapMeshes[i, j].transform.position = new Vector3(i * offset, 0.0f, j * offset);
            }


        }
    }

    public void DrawMHugeMap(MeshData meshData, Texture2D heighMapTexture, Texture2D colorMapTexture, int indice, Vector2 position, int nbChunkBySize)
    {
        /*if(hugeMapMeshes==null)
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
                    hugeMapMeshes[i, j].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
                

            }
        }

        for (int i = 0; i < nbChunkBySize; i++)
        {
            for (int j = 0; j < nbChunkBySize; j++) 
            {
                hugeMapMeshes[i, j].GetComponent<MeshFilter>().mesh = meshData.CreateMesh();
                hugeMapMeshes[i, j].GetComponent<MeshRenderer>().material.mainTexture = texture;
                Vector3 meshPosition = new Vector3(10*i*(MapGenerator.mapChunkSizeMax - 1), 0.0f, 10*j * (MapGenerator.mapChunkSizeMax - 1));
                hugeMapMeshes[i, j].transform.position = meshPosition;  
            }


        }*/

        hugeHeightMapTextureRenderer.material.mainTexture = heighMapTexture;
        hugeHeightMapTextureRenderer.transform.localScale = new Vector3(heighMapTexture.width, 1, heighMapTexture.height);

        hugeColourMapTextureRenderer.sharedMaterial.mainTexture = colorMapTexture;
        hugeColourMapTextureRenderer.transform.localScale = new Vector3(colorMapTexture.width, 1, colorMapTexture.height);




        /*bigMapMeshFilters[indice].sharedMesh = meshData.CreateMesh();
        bigMapMeshRenderers[indice].material.mainTexture = texture;
        bigMapMeshFilters[indice].transform.position = new Vector3(position.x, 0, position.y);
        bigMapMeshRenderers[indice].transform.position = new Vector3(position.x, 0, position.y);*/




    }
}
