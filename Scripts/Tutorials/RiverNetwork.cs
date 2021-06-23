using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverNetwork : MonoBehaviour
{
    public int dim;
    public float disc_radius = 1.0f;
    public float max_delta = 0.05f;
    public float river_downcutting_constant = 1.3f;
    public float directional_inertia = 0.4f;
    public float default_water_level = 1.0f;
    public float evaporation_rate = 0.2f;

    public float[,] GenerateRiverNetwork(Vector2 center, ref float maxLocalNoiseHeight, ref float minLocalNoiseHeight, int size, NoiseData noiseDataMointains, NoiseData noiseDataSimple, NoiseData noiseDataDiff)
    {
        dim = size;
        float[,] result = new float[size, size];

        float[,] landNoise = FindObjectOfType<MapGenerator>().generateNoiseHills(center, size, noiseDataSimple);
        float[,] mountainNoise = FindObjectOfType<MapGenerator>().generateNoiseMountains(center, ref maxLocalNoiseHeight, ref minLocalNoiseHeight, size, noiseDataMointains);
        float[,] diffNoise = FindObjectOfType<MapGenerator>().generateNoiseDiff(center, size, noiseDataDiff);

        bool[,] test = BooleanBump(landNoise, size);
        //bool[,] removeLakes = RemoveLakes(test, size); //TODO

        float[,] mask = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                mask[x, y] = System.Convert.ToSingle(test[x, y]);             
            }
        }

        return diffNoise;
    }

    private bool[,] RemoveLakes(bool[,]  mask, int size) {

        return connectedRegions(mask, size);
    }


    

    private bool[,] BooleanBump(float[,] noiseMap, int size)
    {
        bool[,] test = new bool[size, size];
        float[,] bump = Bump(size, 0.1f * size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                test[x, y] = (noiseMap[x, y] + bump[x, y] - 1.1f) > 0;
                //test[x, y] = (0.5f+ bump[x, y] - 1.1f) > 0;
            }
        }

        return test;
    }

    private float[,] Bump(int size, float sigma)
    {
        int[,] meshgridY = new int[size, size];
        int[,] meshgridX = new int[size, size];
        float[,] hypot = new float[size, size];
        float[,] result = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                meshgridY[x, y] = y;
                meshgridX[x, y] = x;
                hypot[x, y] = Hypot(meshgridX[x, y] - size / 2, meshgridY[x, y] - size / 2);
                float c = size / 2;
                result[x, y] = (float)System.Math.Tanh((double)(Mathf.Max(c - hypot[x, y], 0.0f) / sigma));
            }
        }

        return result;
    }

    private float Hypot(float value1, float value2)
    {
        return Mathf.Sqrt(Mathf.Pow(value1, 2) + Mathf.Pow(value2, 2));
    }


    private bool[,] connectedRegions(bool[,] matrix, int size)
    {
        bool[,] result = new bool[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                result[x, y] = true;
            }
        }


        Queue<System.Tuple<int, int>> coords = new Queue<System.Tuple<int, int>>();

        coords.Enqueue(new System.Tuple<int, int>(0, 0));

        while (coords.Count>0)
        {
            System.Tuple<int, int> coord = coords.Dequeue();

            if (matrix[coord.Item1, coord.Item2] == false)
            {
                result[coord.Item1, coord.Item2] = false;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        
                        int xCoord = coord.Item1 + i;
                        int yCoord = coord.Item2 + j;

                        if(-1 < xCoord && xCoord<size && -1 < yCoord && yCoord < size)
                        {
                            if(matrix[xCoord, yCoord] == false && result[xCoord, xCoord] == true)
                            {
                                coords.Enqueue(new System.Tuple<int, int>(xCoord, yCoord));
                            }
                        }
                    }
                }
            }
        }

        return result;
    }


}
