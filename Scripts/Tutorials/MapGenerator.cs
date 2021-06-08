using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap, BigMesh, HugeMap}; 
    public DrawMode drawMode;

    public enum NoiseMode { Simple, Diff, Land, Moutains, LandAndMountains};
    public NoiseMode noiseMode;

    public enum DrawValue { HeightMap, Gradient, Water, Sediment};
    public DrawValue drawvalue;

    public bool autoUpdate;
    public bool useFalloff;
    public bool useErosion;
    [Range(0, 500)]
    public int erosionIterations;

    public Noise.NormalizeMode normalizeMode;

    

    public const int mapChunkSize = 239; //239
    public int mapChunksBySide;
    [Range(0,6)]
    public int editorPreviewLOD;
    public float noiseScale;


    public int mountainNoiseDivider;

    [Range(0, 1)]
    public float waterLevel;

    [Range(1, 20)]
    public int octaves;
    [Range(0,1)]
    public float persistance;
    [Range(1, 5)]
    public float lacunarity;

    public int seed;
    public Vector2 offset;



    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    

    public TerrainType[] regions;

    float[,] falloffMap;
    float[,] falloffBigMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();



    public int bigMapSize;
    public float maxPossibleHeightForLand; 
    public float maxPossibleHeightFotMountains;
    public Material bigMapMaterial;
    public GameObject bigMapParent;
    public GameObject gradientMapParent;

    private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize+2);
        falloffBigMap = FalloffGenerator.GenerateFalloffMap((mapChunkSize+2)*3);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        


        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.BigMesh)
        {
            //display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));

            /*int indice = 0;

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    Vector2 chunkCoord = new Vector2(xOffset,yOffset);
                    int chunkSize = mapChunkSize-1;
                    Vector2 position = chunkCoord * chunkSize;

                    MapData mapDataForBigMap = GenerateBigMapData(position, chunkCoord);

                    display.DrawMeshByIndice(MeshGenerator.GenerateTerrainMesh(mapDataForBigMap.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapDataForBigMap.colourMap, mapChunkSize, mapChunkSize), indice, position);
                    indice++;
                }
            }*/



        }
        else if (drawMode == DrawMode.HugeMap)
        {
            display.DrawMHugeMap(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize), 0, new Vector2(0.0f, 0.0f),mapChunksBySide);
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            //display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize * 3)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();

    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
        
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue(); 
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = GenerateNoiseMap(center);

        int colorMapChunkSize = mapChunkSize +2;

        Color[] colourMap = new Color[colorMapChunkSize * colorMapChunkSize];
        for (int y = 0; y < colorMapChunkSize; y++)
        {
            for (int x = 0; x < colorMapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;

                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);

    }

    private float[,] GenerateNoiseMap(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

        /*float[,] noiseMap = new float[mapChunkSize + 2, mapChunkSize + 2];
        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                noiseMap[x, y] = (float)(x) / (mapChunkSize + 2);
            }
        }*/


        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        
        if(noiseMode == NoiseMode.Diff)
        {
            float[,] noiseMap1 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap2 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    noiseMap[x, y] = Math.Abs(noiseMap1[x, y] - noiseMap2[x, y]);

                    if (normalizeMode == Noise.NormalizeMode.Global)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(0, maxPossibleHeightForLand, noiseMap[x, y]);
                    }
                }
            }
        }
        else if (noiseMode == NoiseMode.Land)
        {
            float[,] noiseMap1 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap2 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap3 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 2, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap4 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 3, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

            float[,] noiseMap5 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 4, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap6 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 5, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap7 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 6, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap8 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 7, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    float diff1 = Math.Abs(noiseMap1[x, y] - noiseMap2[x, y]);
                    float diff2 = Math.Abs(noiseMap3[x, y] - noiseMap4[x, y]);
                    float diff3 = (noiseMap5[x, y] - noiseMap6[x, y]);
                    float diff4 = (noiseMap7[x, y] - noiseMap8[x, y]);
                    //float noiseHeight = Math.Abs(diff1 - diff2) ;
                    float noiseHeight = diff1;

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            Debug.Log("Land min " + minLocalNoiseHeight + " Land max " + maxLocalNoiseHeight);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    //noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                    if (normalizeMode == Noise.NormalizeMode.Local)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                    else
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(0, maxPossibleHeightForLand, noiseMap[x, y]);
                        //float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                        //noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }

                }
            }
        }

        else if (noiseMode == NoiseMode.Moutains)
        {
            float[,] noiseMap1 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 4, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap2 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 5, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap3 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 6, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap4 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 7, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);

            float[,] noiseMap5 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 4, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap6 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 5, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap7 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 6, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
            float[,] noiseMap8 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 7, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    float diff1 = Math.Abs(noiseMap1[x, y] - noiseMap2[x, y]);
                    float diff2 = Math.Abs(noiseMap3[x, y] - noiseMap4[x, y]);
                    float diff3 = (noiseMap5[x, y] - noiseMap6[x, y]);
                    float diff4 = (noiseMap7[x, y] - noiseMap8[x, y]);
                    float noiseHeight = Math.Abs(diff1 + diff2);

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            Debug.Log("Mt min " + minLocalNoiseHeight + " Mt max " + maxLocalNoiseHeight);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    

                    if (normalizeMode == Noise.NormalizeMode.Global)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(0, maxPossibleHeightFotMountains, noiseMap[x, y]);
                        noiseMap[x, y] = 1 - noiseMap[x, y];
                    }
                    else
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                        noiseMap[x, y] = 1 - noiseMap[x, y];
                    }

                }
            }

        }

        else if (noiseMode == NoiseMode.LandAndMountains)
        {
            noiseMap = generateLandAndMountains(center, noiseMap, ref maxLocalNoiseHeight, ref minLocalNoiseHeight);
        }

        if (useErosion)
        {
            noiseMap = Erosion.GenerateErodedMap(mapChunkSize + 2, mapChunkSize + 2, noiseMap, erosionIterations, drawvalue, gradientMapParent, waterLevel);
        }

        return noiseMap;
    }

    private float[,] generateLandAndMountains(Vector2 center, float[,] noiseMap, ref float maxLocalNoiseHeight, ref float minLocalNoiseHeight)
    {
        float[,] noiseMapResult = new float[mapChunkSize + 2, mapChunkSize + 2];

        float[,] noiseMap1 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap3 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 2, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap4 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 3, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

        float[,] noiseMap5 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 4, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap6 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 5, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap7 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 6, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);
        float[,] noiseMap8 = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed + 7, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode);

        float[,] landNoiseMap = new float[mapChunkSize + 2, mapChunkSize + 2];
        float[,] moutainsNoiseMap = new float[mapChunkSize + 2, mapChunkSize + 2];

        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                float diff1 = Math.Abs(noiseMap1[x, y] - noiseMap2[x, y]);
                float diff2 = Math.Abs(noiseMap3[x, y] - noiseMap4[x, y]);
                //float noiseHeight = Math.Abs(diff1 - diff2);
                float noiseHeight = diff1;

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                landNoiseMap[x, y] = noiseHeight;
            }
        }

        Debug.Log("Ld min " + minLocalNoiseHeight + " Ld max " + maxLocalNoiseHeight);

        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {


                if (normalizeMode == Noise.NormalizeMode.Global)
                {
                    landNoiseMap[x, y] = Mathf.InverseLerp(0, maxPossibleHeightForLand, landNoiseMap[x, y]);
                }
                else
                {
                    landNoiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, landNoiseMap[x, y]);
                }

            }
        }

        maxLocalNoiseHeight = float.MinValue;
        minLocalNoiseHeight = float.MaxValue;


        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                float diff1 = Math.Abs(noiseMap5[x, y] - noiseMap6[x, y]);
                float diff2 = Math.Abs(noiseMap7[x, y] - noiseMap8[x, y]);
                float noiseHeight = Math.Abs(diff1 + diff2);

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                moutainsNoiseMap[x, y] = noiseHeight;
            }
        }

        Debug.Log("Mt min " + minLocalNoiseHeight + " Mt max " + maxLocalNoiseHeight);

        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                //moutainsNoiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, moutainsNoiseMap[x, y]);
                //moutainsNoiseMap[x, y] = 1 - moutainsNoiseMap[x, y];

                if (normalizeMode == Noise.NormalizeMode.Global)
                {
                    moutainsNoiseMap[x, y] = Mathf.InverseLerp(0, maxPossibleHeightFotMountains, moutainsNoiseMap[x, y]);
                    moutainsNoiseMap[x, y] = 1 - moutainsNoiseMap[x, y];
                }
                else
                {
                    moutainsNoiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, moutainsNoiseMap[x, y]);
                    moutainsNoiseMap[x, y] = 1 - moutainsNoiseMap[x, y];
                }

            }
        }

        maxLocalNoiseHeight = float.MinValue;
        minLocalNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                float noiseHeight;

                //noiseHeight = landNoiseMap[x, y] * moutainsNoiseMap[x, y];

                /*if (landNoiseMap[x, y] < mountainLevel)
                {
                    noiseHeight = landWeight * landNoiseMap[x, y];
                }
                else// if (moutainsNoiseMap[x, y] > landNoiseMap[x, y]) 
                {
                    noiseHeight = landWeight * landNoiseMap[x, y] + mountainsWeight * moutainsNoiseMap[x, y];
                }*/

                /*if(moutainsNoiseMap[x, y] > landNoiseMap[x, y] && landNoiseMap[x, y] > waterLevel)
                {
                    noiseHeight = mountainsWeight * moutainsNoiseMap[x, y];
                }
                else
                {
                    noiseHeight = landWeight * landNoiseMap[x, y];
                }*/

                noiseHeight = moutainsNoiseMap[x, y] - (1 - landNoiseMap[x, y]);

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMapResult[x, y] = noiseHeight;
            }

            
        }

        Debug.Log("All min " + minLocalNoiseHeight + " All max " + maxLocalNoiseHeight);

        for (int y = 0; y < mapChunkSize + 2; y++)
        {
            for (int x = 0; x < mapChunkSize + 2; x++)
            {
                if (normalizeMode == Noise.NormalizeMode.Local)
                {
                    noiseMapResult[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMapResult[x, y]);
                }
                else
                {
                    noiseMapResult[x, y] = Mathf.InverseLerp(0, maxPossibleHeightFotMountains, noiseMapResult[x, y]);
                    //float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    //noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

            }
        }

        return noiseMapResult;
    }

    MapData GenerateBigMapData(Vector2 center, Vector2 chunkCoord)
    {
        float[,] noiseMap = GenerateNoiseMap(center);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    int xOffset =  (int)chunkCoord.x * mapChunkSize + mapChunkSize;
                    int yOffset = (int)chunkCoord.y * mapChunkSize + mapChunkSize;
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffBigMap[xOffset + x, yOffset +  y]);
                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;

                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);

    }


    private void OnValidate()
    {
       

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if(octaves < 0)
        {
            octaves = 0;
        }

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
        falloffBigMap = FalloffGenerator.GenerateFalloffMap((mapChunkSize + 2) * 3);

        
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
    public Color colour;


}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
