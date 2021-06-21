using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator 
{
    
    public static float a = 15f;
    public static float b = 2.2f;

    public static float[,] GenerateFalloffMap(int size, float[,] noiseMap, float _a, float _b, float noiseReduction)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                //float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                float value = ((Mathf.Abs(x)+ Mathf.Abs(y))/2 + Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)))/2;
                //float value = Mathf.Sqrt(Mathf.Pow(x,2) + Mathf.Pow(y,2)) + (Mathf.Abs(x) + Mathf.Abs(y)) / 2;


                map[i, j] = (Evaluate(value, _a, _b) - noiseReduction * noiseMap[i, j] * Evaluate(value, _a, _b));
            }
        }

        return map;
    }

    static float Evaluate(float value, float _a, float _b)
    {



        return Mathf.Pow(value, _a) / (Mathf.Pow(value, _a) + Mathf.Pow(_b - _b * value, _a));
    }

    static float Evaluate(float value)
    {
        
        

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

}
