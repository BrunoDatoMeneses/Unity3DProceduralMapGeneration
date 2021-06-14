using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { SingleMap, HugeMap}; 
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


    public const int chunkSizeMax = 241;
    public const int mapChunkSize = 239; //239

    public int hugeMapSize;
    public int nbMapChunksBySide;
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

    public int worldScale = 500;



    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    

    public TerrainType[] regions;

    public Terrain terrainType;

    float[,] falloffMap;
    float[,] falloffugeMap;

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
        falloffugeMap = FalloffGenerator.GenerateFalloffMap(chunkSizeMax * nbMapChunksBySide);
    }

    public void DrawMapInEditor()
    {
        MapData mapData;
        


        if (drawMode == DrawMode.HugeMap)
        {
             
            hugeMapSize = (chunkSizeMax-3) * nbMapChunksBySide +4;
            meshHeightMultiplier = hugeMapSize / 8.0f;
            falloffugeMap = FalloffGenerator.GenerateFalloffMap(hugeMapSize);
            mapData = GenerateHugeMapData(Vector2.zero, hugeMapSize);
            
        }
        else
        {
            meshHeightMultiplier = mapChunkSize / 8.0f;
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);
            mapData = GenerateMapData(Vector2.zero, mapChunkSize +2);
        }


        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.SingleMap)
        {
            display.DrawFallOffTexture(TextureGenerator.TextureFromHeightMap(falloffMap));
            display.DrawHeightTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            display.DrawColourTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));

        }
        else if (drawMode == DrawMode.HugeMap)
        {
            display.DrawHugeFallOffTexture(TextureGenerator.TextureFromHeightMap(falloffugeMap));
            display.DrawHugeHeightTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            display.DrawHugeColourTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, hugeMapSize, hugeMapSize));

            
            int subSize = chunkSizeMax;

            display.DrawHugeMapMeshes(mapData, nbMapChunksBySide, subSize, hugeMapSize);

            /*float[,] subHeighMap = new float[subSize, subSize];
            Color[] subColourMap = new Color[subSize * subSize];
            for (int y = 0; y < subSize; y++)
            {
                for (int x = 0; x < subSize; x++)
                {
                    subHeighMap[x, y] = mapData.heightMap[subSize + x, y];
                    subColourMap[y * subSize + x] = mapData.colourMap[y * hugeMapSize + x + subSize];

                }
            }

            MapData subMapData = new MapData(subHeighMap, subColourMap);


            display.DrawHugeMapMeshes(MeshGenerator.GenerateTerrainMesh(subMapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(subMapData.colourMap, subSize, subSize), mapChunksBySide);*/


            //display.DrawMHugeMap(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromHeightMap(mapData.heightMap), 0, new Vector2(0.0f, 0.0f),mapChunksBySide);
            /*display.DrawMHugeMap(null, 
                TextureGenerator.TextureFromHeightMap(mapData.heightMap), 
                TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize), 
                0, 
                new Vector2(0.0f, 0.0f), 
                mapChunksBySide);*/

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
        MapData mapData = GenerateMapData(center, mapChunkSize + 2);
        Debug.Log("mapData Chunk " + (mapChunkSize + 2));
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
        Debug.Log("Heigh map before mesh " + mapData.heightMap.GetLength(0));
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


    MapData GenerateMapData(Vector2 center, int size)
    {
        float[,] noiseMap = GenerateNoiseMap(center, size);

        int colorMapChunkSize = size;

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

    MapData GenerateHugeMapData(Vector2 center, int hugeMapSize)
    {
        float[,] noiseMap = GenerateNoiseMap(center, hugeMapSize);

        int colorMapChunkSize = hugeMapSize;

        Color[] colourMap = new Color[colorMapChunkSize * colorMapChunkSize];
        for (int y = 0; y < colorMapChunkSize; y++)
        {
            for (int x = 0; x < colorMapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffugeMap[x, y]);
                }

                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * hugeMapSize + x] = regions[i].colour;

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

    private float[,] GenerateNoiseMap(Vector2 center, int size)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(size, size, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

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
            noiseMap = generateNoiseDiff(center, size);
        }
        else if (noiseMode == NoiseMode.Land)
        {
            noiseMap = generateNoiseLand(center, ref maxLocalNoiseHeight, ref minLocalNoiseHeight, size);
        }

        else if (noiseMode == NoiseMode.Moutains)
        {
            noiseMap = generateNoiseMountains(center, ref maxLocalNoiseHeight, ref minLocalNoiseHeight, size);
        }
        else if (noiseMode == NoiseMode.LandAndMountains)
        {
            noiseMap = generateLandAndMountains(center, ref maxLocalNoiseHeight, ref minLocalNoiseHeight, size);
        }

        if (useErosion)
        {
            Erosion2 erosion2 = FindObjectOfType<Erosion2>();
            //erosion.GenerateErodedMap(size, size, noiseMap, erosionIterations, drawvalue, gradientMapParent, waterLevel);
            noiseMap = erosion2.GenerateErodedMap(size, size, noiseMap, erosionIterations, drawvalue, gradientMapParent, waterLevel);
        }

        return noiseMap;
    }

    private float[,] generateNoiseMountains(Vector2 center, ref float maxLocalNoiseHeight, ref float minLocalNoiseHeight, int size)
    {
        float[,] noiseMoutains = new float[size, size];

        float[,] noiseMap1 = Noise.GenerateNoiseMap(size, size, seed + 4, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(size, size, seed + 5, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap3 = Noise.GenerateNoiseMap(size, size, seed + 6, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap4 = Noise.GenerateNoiseMap(size, size, seed + 7, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        float[,] noiseMap5 = Noise.GenerateNoiseMap(size, size, seed + 4, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap6 = Noise.GenerateNoiseMap(size, size, seed + 5, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap7 = Noise.GenerateNoiseMap(size, size, seed + 6, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap8 = Noise.GenerateNoiseMap(size, size, seed + 7, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

                noiseMoutains[x, y] = noiseHeight;
            }
        }

        Debug.Log("Mt min " + minLocalNoiseHeight + " Mt max " + maxLocalNoiseHeight);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {


                if (normalizeMode == Noise.NormalizeMode.Global)
                {
                    noiseMoutains[x, y] = Mathf.InverseLerp(0, maxPossibleHeightFotMountains, noiseMoutains[x, y]);
                    noiseMoutains[x, y] = 1 - noiseMoutains[x, y];
                }
                else
                {
                    noiseMoutains[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMoutains[x, y]);
                    noiseMoutains[x, y] = 1 - noiseMoutains[x, y];
                }

            }
        }

        return noiseMoutains;
    }

    private float[,] generateNoiseLand(Vector2 center, ref float maxLocalNoiseHeight, ref float minLocalNoiseHeight, int size)
    {
        float[,] noiseLand = new float[size, size];

        float[,] noiseMap1 = Noise.GenerateNoiseMap(size, size, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(size, size, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap3 = Noise.GenerateNoiseMap(size, size, seed + 2, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap4 = Noise.GenerateNoiseMap(size, size, seed + 3, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        float[,] noiseMap5 = Noise.GenerateNoiseMap(size, size, seed + 4, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap6 = Noise.GenerateNoiseMap(size, size, seed + 5, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap7 = Noise.GenerateNoiseMap(size, size, seed + 6, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap8 = Noise.GenerateNoiseMap(size, size, seed + 7, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

                noiseLand[x, y] = noiseHeight;
            }
        }

        Debug.Log("Land min " + minLocalNoiseHeight + " Land max " + maxLocalNoiseHeight);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                if (normalizeMode == Noise.NormalizeMode.Local)
                {
                    noiseLand[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseLand[x, y]);
                }
                else
                {
                    noiseLand[x, y] = Mathf.InverseLerp(0, maxPossibleHeightForLand, noiseLand[x, y]);
                    //float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    //noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

            }
        }

        return noiseLand;
    }

    private float[,] generateNoiseDiff(Vector2 center, int size)
    {
        float[,] noiseDiff = new float[size, size];

        float[,] noiseMap1 = Noise.GenerateNoiseMap(size, size, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(size, size, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseDiff[x, y] = Math.Abs(noiseMap1[x, y] - noiseMap2[x, y]);

                if (normalizeMode == Noise.NormalizeMode.Global)
                {
                    noiseDiff[x, y] = Mathf.InverseLerp(0, maxPossibleHeightForLand, noiseDiff[x, y]);
                }
            }
        }

 
        return noiseDiff;
    }

    private float[,] generateLandAndMountains(Vector2 center, ref float maxLocalNoiseHeight, ref float minLocalNoiseHeight, int size)
    {
        float[,] noiseMapResult = new float[size, size];

        float[,] noiseMap1 = Noise.GenerateNoiseMap(size, size, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(size, size, seed + 1, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap3 = Noise.GenerateNoiseMap(size, size, seed + 2, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap4 = Noise.GenerateNoiseMap(size, size, seed + 3, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        float[,] noiseMap5 = Noise.GenerateNoiseMap(size, size, seed + 4, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap6 = Noise.GenerateNoiseMap(size, size, seed + 5, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap7 = Noise.GenerateNoiseMap(size, size, seed + 6, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);
        float[,] noiseMap8 = Noise.GenerateNoiseMap(size, size, seed + 7, noiseScale / mountainNoiseDivider, octaves, persistance, lacunarity, center + offset, normalizeMode, worldScale);

        float[,] landNoiseMap = new float[size, size];
        float[,] moutainsNoiseMap = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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


        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
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
        float[,] noiseMap = GenerateNoiseMap(center, mapChunkSize +2);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    int xOffset =  (int)chunkCoord.x * mapChunkSize + mapChunkSize;
                    int yOffset = (int)chunkCoord.y * mapChunkSize + mapChunkSize;
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffugeMap[xOffset + x, yOffset +  y]);
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
        falloffugeMap = FalloffGenerator.GenerateFalloffMap(chunkSizeMax * nbMapChunksBySide);


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
