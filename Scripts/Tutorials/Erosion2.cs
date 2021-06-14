using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class Erosion2 : MonoBehaviour
{


    // Grid dimension constants
    public  int full_width_set = 200;
    public  int dim = 512;
    public  double cell_width;
    public  double cell_area;

    // Water-related constants
    public  double rain_rate_set = 0.0008d;
    public  double evaporation_rate_set = 0.0005d;

    public  double rain_rate;

    // Slope constants
    public  double min_height_delta_set = 0.05d;
    public  double repose_slope_set = 0.03d;
    public  double gravity_set = 30.0d;
    //public  double gradient_sigma = 0.5d;

    // Sediment constants
    public  double sediment_capacity_constant_set = 50.0d;//50.0d;
    public  double dissolving_rate_set = 0.25d;
    public  double deposition_rate_set = 0.001d;

    // The numer of iterations is proportional to the grid dimension. This is to 
    // allow changes on one side of the grid to affect the other side.
    public  int iterations;

    public  int traceLevel = 15;


    private void OnValidate()
    {
        /*full_width = 1;
        dim = 3;*/

        cell_width = (double)((double)full_width_set / (double)dim);
        cell_area = cell_width * cell_width;
        rain_rate = rain_rate_set * cell_area;
        iterations = 0;

        Debug.Log("ON VALIDATE");

        /*int mapSize = 3;
        double[,] terrainTest = new double[mapSize, mapSize];
        terrainTest[0, 0] = 0.24445265d;
        terrainTest[0, 1] = 0.0d;
        terrainTest[0, 2] = 0.80583694d;

        terrainTest[1, 0] = 0.75046315d;
        terrainTest[1, 1] = 0.7452318d;
        terrainTest[1, 2] = 0.91508314d;

        terrainTest[2, 0] = 1.0d;
        terrainTest[2, 1] = 0.11759199d;
        terrainTest[2, 2] = 0.69027027d;

        PrintTabFlt(terrainTest, "terrainTest");
        // `sediment` is the amount of suspended "dirt" in the water. Terrain will be
        // transfered to/from sediment depending on a number of different factors.
        double[,] sediment = new double[mapSize, mapSize];

        // The amount of water. Responsible for carrying sediment.
        double[,] water = new double[mapSize, mapSize];

        // The water velocity.
        double[,] velocity = new double[mapSize, mapSize];

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                sediment[x, y] = 0.0d;
                water[x, y] = 0.0d;
                velocity[x, y] = 0.0d;
            }
        }
        water[0, 0] = 0.24445265d;
        water[0, 1] = 0.0d;
        water[0, 2] = 0.80583694d;

        water[1, 0] = 0.75046315d;
        water[1, 1] = 0.7452318d;
        water[1, 2] = 0.91508314d;

        water[2, 0] = 1.0d;
        water[2, 1] = 0.11759199d;
        water[2, 2] = 0.69027027d;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                // Add precipitation. This is done by via simple uniform random distribution,
                // although other models use a raindrop model
                //water[x, y] += Random.Range(0.0f, 1.0f) * rain_rate;
                water[x, y] *=  rain_rate;
            }
        }*/

        /*# Compute the normalized gradient of the terrain height to determine where
        # water and sediment will be moving.*/
        //UnityEngine.Vector2[,] gradient = getSimpleGradient(mapSize, mapSize, terrainTest);

        /*Complex[,] cGradient = getSimpleGradientC(mapSize, mapSize, terrainTest);
        PrintTabComplex(cGradient, "cgradient");

        double[,] neighbor_height = sampleCplx(terrainTest, cGradient, mapSize);
        double[,] height_delta = new double[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                height_delta[x, y] = terrainTest[x, y] - neighbor_height[x, y];
            }
        }

        PrintTabFlt(height_delta, "height_delta");

        // Update velocity TODO plus loin
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                velocity[x, y] = gravity * height_delta[x, y] / cell_width;
            }
        }

        PrintTabFlt(velocity, "velocity");*/

        /*# The sediment capacity represents how much sediment can be suspended in
        # water. If the sediment exceeds the quantity, then it is deposited,
        # otherwise terrain is eroded.*/

        /*double[,] sediment_capacity = new double[mapSize, mapSize];
        double[,] deposited_sediment = new double[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                sediment_capacity[x, y] = maxDouble((double)height_delta[x, y], (double)min_height_delta) / (double)cell_width * (double)velocity[x, y] * water[x, y] * sediment_capacity_constant;
                if (height_delta[x, y] < 0)
                {
                    deposited_sediment[x, y] = minDouble(height_delta[x, y], sediment[x, y]);
                }
                else if(sediment[x, y] > sediment_capacity[x, y])
                {
                    deposited_sediment[x, y] = deposition_rate * (sediment[x, y] - sediment_capacity[x, y]);
                }
                else //If sediment <= sediment_capacity
                {
                    deposited_sediment[x, y] = dissolving_rate * (sediment[x, y] - sediment_capacity[x, y]);
                }

                //Don't erode more sediment than the current terrain height.
                deposited_sediment[x, y] = maxDouble(-height_delta[x, y], deposited_sediment[x, y]);
            }
        }

        PrintTabFlt(sediment_capacity, "sediment_capacity");
        PrintTabFlt(deposited_sediment, "deposited_sediment");

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                sediment[x, y] -= deposited_sediment[x, y];
                terrainTest[x, y] += deposited_sediment[x, y];
            }
        }

        PrintTabFlt(sediment, "sediment");
        PrintTabFlt(terrainTest, "terrainTest");

        sediment = Displace(sediment, cGradient, mapSize);
        PrintTabFlt(sediment, "displacedSediment");

        water = Displace(water, cGradient, mapSize);
        PrintTabFlt(water, "displacedWater");*/

        //# Smooth out steep slopes.
        //terrainTest = ApplySlippage(terrainTest, repose_slope, cell_width, mapSize);



        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                //# Update velocity 
                velocity[x, y] = gravity * height_delta[x, y] / cell_width;
                //# Apply evaporation
                water[x, y] *= 1 - evaporation_rate;
            }
        }

        PrintTabFlt(velocity, "velocity");*/






    }

    private  double[,] ApplySlippage(double[,] terrain, double repose_slope, double cell_width, int mapSize)
    {
        double[,] result = new double[mapSize, mapSize];
        Complex[,] delta = getSimpleGradientCWithoutNormalization(mapSize, mapSize, terrain);
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                delta[x, y] /= cell_width;
            }
        }

        double[,] smoothed = gaussianBlur(terrain, 1.5f, mapSize);
        bool[,] should_smouth = new bool[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (delta[x, y].Magnitude > repose_slope)
                {
                    result[x, y] = smoothed[x, y];
                }
                else
                {
                    result[x, y] = terrain[x, y];
                }
            }
        }

        return result;
    }

    //# Peforms a gaussian blur of `a`.
    private  double[,] gaussianBlur(double[,] a, double sigma, int mapSize)
    {
        double[,] result = new double[mapSize, mapSize];
        //TODO

        return a;
    }



    private  double Fns(int i, double x)
    {
        if (i == -1)
        {
            return -x;
        }
        else if (i == 0)
        {
            return 1 - System.Math.Abs(x);
        }
        else if (i == 1)
        {
            return x;
        }
        else
        {
            return 0;
        }
    }



    private  double[,] Displace2(double[,] a, Complex[,] delta, int mapSize)
    {
        double[,] result = new double[mapSize, mapSize];
        double[,] oldA = new double[mapSize, mapSize];
        double[,] displaced = new double[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                result[x, y] = 0.0d;
                oldA[x, y] = a[x, y];

                double valueX = 0.0d;
                //double valueY = 0.0d;
                for (int dx = -1; dx < 2; dx++)
                {
                    for (int dy = -1; dy < 2; dy++)
                    {
                        /*int modifiedCoordX = x + dx;
                        int modifiedCoordY = y + dy;
                        if(modifiedCoordX < 0)
                        {
                            modifiedCoordX = mapSize - 1;
                        }else if(modifiedCoordX > mapSize - 1)
                        {
                            modifiedCoordX = 0;
                        }

                        if (modifiedCoordY < 0)
                        {
                            modifiedCoordY = mapSize - 1;
                        }
                        else if (modifiedCoordY > mapSize - 1)
                        {
                            modifiedCoordY = 0;
                        }*/

                        //valueX -= a[modifiedCoordX, modifiedCoordY] * Fns(dx, delta[modifiedCoordX, y].Imaginary) * Fns(dy, delta[x, modifiedCoordY].Real);

                        if (0 <= x + dx && x + dx < mapSize && 0 <= y + dy && y + dy < mapSize)
                        {
                            valueX -= a[x + dx, y + dy] * Fns(dx, delta[x + dx, y].Imaginary) * Fns(dy, delta[x, y + dy].Real);

                        }



                    }
                }

                result[x, y] = valueX;
            }
        }



        return result;
    }

    private  double[,] Displace(double[,] a, Complex[,] delta, int mapSize, string tag)
    {
        double[,] result = new double[mapSize, mapSize];

        PrintTabDouble(a, "a before ", 0, mapSize);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                result[x, y] = 0.0d;

            }
        }
        for (int dx = -1; dx < 2; dx++)
        {
            double[,] wx = new double[mapSize, mapSize];
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    wx[x, y] = maxDouble(Fns(dx, (double)delta[x, y].Real), 0.0f);

                }
            }

            PrintTabDouble(wx, "sediment wx " + dx, 0, mapSize);

            for (int dy = -1; dy < 2; dy++)
            {
                double[,] wy = new double[mapSize, mapSize];
                double[,] wProductWIthA = new double[mapSize, mapSize];
                double[,] rollDyAxis0 = new double[mapSize, mapSize];

                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        wy[x, y] = maxDouble(Fns(dy, (double)delta[x, y].Imaginary), 0.0f);
                        wProductWIthA[x, y] = wx[x, y] * wy[x, y] * a[x, y];
                    }
                }

                PrintTabDouble(wy, "sediment wy " + dx + " " + dy, 0, mapSize);


                /*for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        wProductWIthA[x, y] = wx[x, y] * wy[x, y] * a[x, y];
                    }
                }*/
                PrintTabDouble(wProductWIthA, "wProductWIthA " + dx + " " + dy, 0, mapSize);


                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (x - dy < 0)
                        {
                            rollDyAxis0[x, y] = wProductWIthA[mapSize - 1, y];
                            //rollDyAxis0[x, y] = wProductWIthA[x, y];
                            //rollDyAxis0[x, y] = 0.0d;
                        }
                        else if (x - dy >= mapSize)
                        {
                            rollDyAxis0[x, y] = wProductWIthA[0, y];
                            //rollDyAxis0[x, y] = wProductWIthA[x, y];
                            //rollDyAxis0[x, y] = 0.0d;
                        }
                        else
                        {
                            rollDyAxis0[x, y] = wProductWIthA[x - dy, y];
                        }

                    }
                }

                PrintTabDouble(rollDyAxis0, "rollDyAxis0 " + dx + " " + dy, 0, mapSize);

                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (y - dx < 0)
                        {
                            result[x, y] += rollDyAxis0[x, mapSize - 1];
                            //result[x, y] += rollDyAxis0[x, y];
                            //result[x, y] += 0.0d;
                        }
                        else if (y - dx >= mapSize)
                        {
                            result[x, y] += rollDyAxis0[x, 0];
                            //result[x, y] += rollDyAxis0[x, y];
                            //result[x, y] += 0.0d;
                        }
                        else
                        {
                            result[x, y] += rollDyAxis0[x, y - dx];
                        }

                    }
                }

                PrintTabDouble(result, "result " + dx + " " + dy, 0, mapSize);

            }
        }

        return result;
    }

    private  void PrintTabDouble(double[,] tab, string name, int lvl, int tabSize)
    {

        if (lvl > traceLevel)
        {
            Debug.Log(name);

            for (int x = 0; x < tabSize; x++)
            {
                for (int y = 0; y < tabSize; y++)
                {
                    Debug.Log(name + " [" + x + "," + y + "] " + tab[x, y]);
                }
            }
        }

    }

    private  void PrintTabFloat(float[,] tab, string name, int lvl, int tabSize)
    {

        if (lvl > traceLevel)
        {
            Debug.Log(name);

            for (int x = 0; x < tabSize; x++)
            {
                for (int y = 0; y < tabSize; y++)
                {
                    Debug.Log(name + " [" + x + "," + y + "] " + tab[x, y]);
                }
            }
        }

    }



    private  void PrintTabInt(int[,] tab, string name)
    {
        /*Debug.Log(name);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Debug.Log(name + " [" + x + "," + y + "] " + tab[x, y]);
            }
        }*/
    }

    private  void PrintTabComplex(Complex[,] tab, string name, int lvl, int tabSize)
    {

        if (lvl > traceLevel)
        {
            Debug.Log(name);

            for (int x = 0; x < tabSize; x++)
            {
                for (int y = 0; y < tabSize; y++)
                {
                    Debug.Log(name + " [" + x + "," + y + "] " + tab[x, y]);
                }
            }
        }


    }

    private  void PrintTabVec(UnityEngine.Vector2[,] tab, string name)
    {
        /*Debug.Log(name);
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Debug.Log(name +" [" + x + "," + y + "] " + tab[x, y].y + " , " + tab[x, y].x);
            }
        }*/

    }

    private  double maxDouble(double a, double b)
    {
        return (a > b) ? a : b;
    }

    private  double minDouble(double a, double b)
    {
        return (a < b) ? a : b;
    }

    public  float[,] GenerateErodedMap(int mapWidth, int mapHeight, float[,] noiseMap, int iterations_, MapGenerator.DrawValue drawvalue, GameObject gradientMapParent, double waterLevel)
    {

        cell_width = (double)((double)full_width_set / (double)mapWidth);
        cell_area = cell_width * cell_width;
        rain_rate = rain_rate_set * cell_area;

        float[,] resultdMap = new float[mapWidth, mapHeight];
        dim = mapWidth;
        int mapSize = mapWidth;
        //`terrain` represents the actual terrain height we're interested in
        double[,] terrainTest = new double[mapWidth, mapHeight];

        //PrintTabFlt(terrainTest, "terrainTest");
        // `sediment` is the amount of suspended "dirt" in the water. Terrain will be
        // transfered to/from sediment depending on a number of different factors.
        double[,] sediment = new double[mapSize, mapSize];

        // The amount of water. Responsible for carrying sediment.
        double[,] water = new double[mapSize, mapSize];

        // The water velocity.
        double[,] velocity = new double[mapSize, mapSize];

        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0.0f;

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                terrainTest[x, y] = noiseMap[x, y];

                sediment[x, y] = 0.0d;
                water[x, y] = 0.0d;
                velocity[x, y] = 0.0d;

                resultdMap[x, y] = (float)terrainTest[x, y];
                min = minDouble(min, resultdMap[x, y]);
                max = maxDouble(max, resultdMap[x, y]);
                sum += resultdMap[x, y];
            }
        }
        Debug.Log(min);
        Debug.Log(max);
        Debug.Log(sum);

        PrintTabDouble(terrainTest, "terrainTest first", 10, mapSize);


        /*for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                sediment[x, y] = 0.0d;
                water[x, y] = 0.0d;
                velocity[x, y] = 0.0d;
            }
        }*/


        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                resultdMap[x, y] = (float)terrainTest[x, y];
                min = minDouble(min, resultdMap[x, y]);
                max = maxDouble(max, resultdMap[x, y]);
                sum += resultdMap[x, y];
            }
        }*/


        for (int i = 0; i < iterations_; i++)
        {
            Debug.Log((i + 1) + "/" + iterations_);

            //water[(int)mapSize/2, (int)mapSize / 2] +=  rain_rate;
            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    // Add precipitation. This is done by via simple uniform random distribution,
                    // although other models use a raindrop model
                    water[x, y] += Random.Range(0.0f, 1.0f) * rain_rate;

                    /*if ((x + y) % 2 == 0) {
                        water[x,y] += rain_rate; // single source
                    }*/

                }
            }

            PrintTabDouble(water, "water", 0, mapSize);

            /*# Compute the normalized gradient of the terrain height to determine where
            # water and sediment will be moving.*/
            //UnityEngine.Vector2[,] gradient = getSimpleGradient(mapSize, mapSize, terrainTest);
            Complex[,] cGradient = getSimpleGradientC(mapSize, mapSize, terrainTest);
            PrintTabComplex(cGradient, "cgradient " + i, 10, mapSize);
            PrintTabDouble(terrainTest, "terrainTest afterGradient", 10, mapSize);

            //# Compute the difference between teh current height the height offset by
            //# `gradient`.
            double[,] neighbor_height = sampleCplx(terrainTest, cGradient, mapSize);
            double[,] height_delta = new double[mapSize, mapSize];

            /*# The sediment capacity represents how much sediment can be suspended in
            # water. If the sediment exceeds the quantity, then it is deposited,
            # otherwise terrain is eroded.*/

            double[,] sediment_capacity = new double[mapSize, mapSize];
            double[,] deposited_sediment = new double[mapSize, mapSize];

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    height_delta[x, y] = terrainTest[x, y] - neighbor_height[x, y];

                    sediment_capacity[x, y] = (maxDouble(height_delta[x, y], min_height_delta_set) / (double)cell_width) * (double)velocity[x, y] * water[x, y] * sediment_capacity_constant_set;


                    if (height_delta[x, y] < 0)
                    {
                        deposited_sediment[x, y] = minDouble(height_delta[x, y], sediment[x, y]);
                    }
                    else if (sediment[x, y] > sediment_capacity[x, y])
                    {
                        deposited_sediment[x, y] = deposition_rate_set * (sediment[x, y] - sediment_capacity[x, y]);
                    }
                    else //If sediment <= sediment_capacity
                    {
                        deposited_sediment[x, y] = dissolving_rate_set * (sediment[x, y] - sediment_capacity[x, y]);
                    }

                    //Don't erode more sediment than the current terrain height.
                    deposited_sediment[x, y] = maxDouble(-height_delta[x, y], deposited_sediment[x, y]);



                    sediment[x, y] -= deposited_sediment[x, y];
                    if (terrainTest[x, y] > waterLevel)
                    {
                        terrainTest[x, y] += deposited_sediment[x, y];
                    }

                }
            }

            PrintTabDouble(neighbor_height, "neighbor_height", 10, mapSize);
            PrintTabDouble(height_delta, "height_delta", 10, mapSize);
            PrintTabDouble(velocity, "velocity", 0, mapSize);


            /*for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    sediment_capacity[x, y] = (maxDouble(height_delta[x, y], min_height_delta) / (double)cell_width) * (double)velocity[x, y] * water[x, y] * sediment_capacity_constant;

                    
                    if (height_delta[x, y] < 0)
                    {
                        deposited_sediment[x, y] = minDouble(height_delta[x, y], sediment[x, y]);
                    }
                    else if (sediment[x, y] > sediment_capacity[x, y])
                    {
                        deposited_sediment[x, y] = deposition_rate * (sediment[x, y] - sediment_capacity[x, y]);
                    }
                    else //If sediment <= sediment_capacity
                    {
                        deposited_sediment[x, y] = dissolving_rate * (sediment[x, y] - sediment_capacity[x, y]);
                    }

                    //Don't erode more sediment than the current terrain height.
                    deposited_sediment[x, y] = maxDouble(-height_delta[x, y], deposited_sediment[x, y]);



                    sediment[x, y] -= deposited_sediment[x, y];
                    terrainTest[x, y] += deposited_sediment[x, y];




                }
            }*/



            PrintTabDouble(sediment_capacity, "sediment_capacity", 10, mapSize);
            PrintTabDouble(deposited_sediment, "deposited_sediment", 10, mapSize);

            PrintTabDouble(sediment, "sediment", 10, mapSize);
            PrintTabDouble(terrainTest, "terrainTest", 0, mapSize);


            PrintTabDouble(sediment, "sedimentbefore", 0, mapSize);
            PrintTabDouble(water, "waterbefore", 0, mapSize);

            sediment = Displace(sediment, cGradient, mapSize, "sediment");
            water = Displace(water, cGradient, mapSize, "water");

            PrintTabDouble(sediment, "sedimentafter", 0, mapSize);
            PrintTabDouble(water, "waterafter", 0, mapSize);
            PrintTabDouble(terrainTest, "terrainTest", 0, mapSize);

            //# Smooth out steep slopes.
            //terrainTest = ApplySlippage(terrainTest, repose_slope, cell_width, mapSize);



            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    //# Update velocity 
                    velocity[x, y] = gravity_set * height_delta[x, y] / cell_width;
                    //# Apply evaporation
                    water[x, y] *= 1 - evaporation_rate_set;
                    //Debug.Log(water[x, y]);
                }
            }

            PrintTabDouble(velocity, "velocity", 0, mapSize);
            PrintTabDouble(water, "water evap", 10, mapSize);
            PrintTabDouble(terrainTest, "terrainTest" + i, 10, mapSize);
        }

        PrintTabDouble(water, "water final", 10, mapSize);
        PrintTabDouble(terrainTest, "terrainTestFinal", 10, mapSize);

        Complex[,] lastGradient = getSimpleGradientCWithoutNormalization(mapSize, mapSize, terrainTest);

        min = float.MaxValue;
        max = float.MinValue;
        sum = 0.0f;
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (drawvalue == MapGenerator.DrawValue.HeightMap)
                {
                    resultdMap[x, y] = (float)terrainTest[x, y];
                }
                else if (drawvalue == MapGenerator.DrawValue.Gradient)
                {
                    resultdMap[x, y] = (float)lastGradient[x, y].Magnitude / 0.08f;
                    /*GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cylinder.transform.parent = gradientMapParent.transform;
                    cylinder.transform.position = new Vector3(x, 0.0f, y);
                    cylinder.transform.localScale = new Vector3(0.1f,0.5f,0.1f);
                    cylinder.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f); */


                }
                else if (drawvalue == MapGenerator.DrawValue.Water)
                {
                    resultdMap[x, y] = (float)(water[x, y]) / 0.02f;
                }
                else if (drawvalue == MapGenerator.DrawValue.Sediment)
                {
                    resultdMap[x, y] = (float)(-sediment[x, y]) / 1.0f;
                }

                min = minDouble(min, resultdMap[x, y]);
                max = maxDouble(max, resultdMap[x, y]);
                sum += resultdMap[x, y];
            }
        }
        Debug.Log("min " + min);
        Debug.Log("max " + max);
        Debug.Log("sum " + sum);

        PrintTabFloat(resultdMap, "resultdMap", 10, mapSize);

        return resultdMap;
    }


    private  UnityEngine.Vector2 simpleGradient(int mapHeight, double[,] terrain, int y, int x)
    {
        UnityEngine.Vector2 gradientValue = new UnityEngine.Vector2(0.0f, 0.0f);
        double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[mapHeight - 1, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[0, y]));
        double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, mapHeight - 1]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, 0]));
        gradientValue.x = (float)dx;
        gradientValue.y = (float)dy;
        if (Mathf.Sqrt(gradientValue.SqrMagnitude()) < 0.1)
        {
            gradientValue.x = Random.Range(0.0f, 1.0f);
            gradientValue.y = Random.Range(0.0f, 1.0f);
        }
        gradientValue.Normalize();
        //gradient[x, y].x /= Mathf.Abs(gradient[x, y].x);
        //gradient[x, y].y /= Mathf.Abs(gradient[x, y].y);
        return gradientValue;
    }

    private  Complex simpleGradientCWithoutNormalization(int mapHeight, double[,] terrain, int y, int x)
    {
        double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[x + 1, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[x - 1, y]));
        double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, y + 1]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, y - 1]));

        //double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[mapHeight - 1, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[0, y]));
        //double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, mapHeight - 1]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, 0]));

        //double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[x, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[x, y]));
        //double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, y]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, y]));

        Complex gradientValue = new Complex(dy, dx);

        //Debug.Log("gradientWithoutNormalization" + " [" + x + "," + y + "] " + gradientValue);

        return gradientValue;
    }

    private  Complex simpleGradientC(int mapHeight, double[,] terrain, int y, int x)
    {
        // wrap world
        double dx = 0.5d * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[mapHeight - 1, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[0, y]));
        double dy = 0.5d * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, mapHeight - 1]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, 0]));

        //double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[x, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[x, y]));
        //double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, y]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, y]));


        //double dx = 0.5f * ((x - 1 >= 0 ? terrain[x - 1, y] : terrain[x + 1, y]) - (x + 1 < mapHeight ? terrain[x + 1, y] : terrain[x - 1, y]));
        //double dy = 0.5f * ((y - 1 >= 0 ? terrain[x, y - 1] : terrain[x, y + 1]) - (y + 1 < mapHeight ? terrain[x, y + 1] : terrain[x, y - 1]));


        Complex gradientValue = new Complex(dy, dx);

        //Debug.Log("gradientWithoutNoise" + " [" + x + "," + y + "] " + gradientValue);

        //Debug.Log("magn " + x + " "+ y +" " + gradientValue.Magnitude); 
        if (gradientValue.Magnitude < 1e-10)
        {
            gradientValue = Complex.Exp(2 * Complex.ImaginaryOne * Mathf.PI * Random.Range(0.0f, 1.0f));
        }
        //Debug.Log("gradientWithNoise" + " [" + x + "," + y + "] " + gradientValue);
        gradientValue /= gradientValue.Magnitude;
        //gradient[x, y].x /= Mathf.Abs(gradient[x, y].x);
        //gradient[x, y].y /= Mathf.Abs(gradient[x, y].y);
        return gradientValue;
    }

    private  UnityEngine.Vector2[,] getSimpleGradient(int mapWidth, int mapHeight, double[,] noiseMap)
    {
        UnityEngine.Vector2[,] gradient = new UnityEngine.Vector2[mapWidth, mapHeight];
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapHeight; x++)
            {

                //Compute the normalized gradient of the terrain height to determine where 
                // water and sediment will be moving.
                gradient[x, y] = simpleGradient(mapHeight, noiseMap, y, x);


            }
        }

        return gradient;
    }

    private  Complex[,] getSimpleGradientC(int mapWidth, int mapHeight, double[,] noiseMap)
    {
        Complex[,] gradient = new Complex[mapWidth, mapHeight];
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapHeight; x++)
            {

                //Compute the normalized gradient of the terrain height to determine where 
                // water and sediment will be moving.
                gradient[x, y] = simpleGradientC(mapHeight, noiseMap, y, x);


            }
        }

        return gradient;
    }

    private  Complex[,] getSimpleGradientCWithoutNormalization(int mapWidth, int mapHeight, double[,] noiseMap)
    {
        Complex[,] gradient = new Complex[mapWidth, mapHeight];
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapHeight; x++)
            {

                //Compute the normalized gradient of the terrain height to determine where 
                // water and sediment will be moving.
                gradient[x, y] = simpleGradientCWithoutNormalization(mapHeight, noiseMap, y, x);


            }
        }

        return gradient;
    }

    private  double lerp(double x, double y, double a)
    {
        double result = (1.0f - a) * x + a * y;
        //Debug.Log(x + " " + y + " " + a + " " + result);
        return result;
    }

    public  double[,] sample(double[,] a, UnityEngine.Vector2[,] offset, int mapSize)
    {
        UnityEngine.Vector2[,] delta = new UnityEngine.Vector2[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                delta[x, y] = -offset[x, y];
            }
        }
        PrintTabVec(delta, "-gradient");

        double[,] mesggridDy = new double[mapSize, mapSize];
        double[,] mesggridDx = new double[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                mesggridDy[x, y] = y;
                mesggridDx[x, y] = x;
            }
        }

        PrintTabDouble(mesggridDy, "mesggridDy", 0, mapSize);
        PrintTabDouble(mesggridDx, "mesggridDx", 0, mapSize);

        double[,] coordsDy = new double[mapSize, mapSize];
        double[,] coordsDx = new double[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                coordsDy[x, y] = mesggridDy[x, y] - delta[x, y].y;
                coordsDx[x, y] = mesggridDx[x, y] - delta[x, y].x;
            }
        }

        PrintTabDouble(coordsDy, "coordsDy", 0, mapSize);
        PrintTabDouble(coordsDx, "coordsDx", 0, mapSize);

        int[,] lowerCoordsDy = new int[mapSize, mapSize];
        int[,] lowerCoordsDx = new int[mapSize, mapSize];
        int[,] upperCoordsDy = new int[mapSize, mapSize];
        int[,] upperCoordsDx = new int[mapSize, mapSize];
        double[,] coord_offsetsDy = new double[mapSize, mapSize];
        double[,] coord_offsetsDx = new double[mapSize, mapSize];

        int[,] lowerCoordsDyMod = new int[mapSize, mapSize];
        int[,] lowerCoordsDxMod = new int[mapSize, mapSize];
        int[,] upperCoordsDyMod = new int[mapSize, mapSize];
        int[,] upperCoordsDxMod = new int[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                lowerCoordsDy[x, y] = Mathf.FloorToInt((float)coordsDy[x, y]);
                lowerCoordsDx[x, y] = Mathf.FloorToInt((float)coordsDx[x, y]);
                upperCoordsDy[x, y] = lowerCoordsDy[x, y] + 1;
                upperCoordsDx[x, y] = lowerCoordsDx[x, y] + 1;
                coord_offsetsDy[x, y] = coordsDy[x, y] - (double)lowerCoordsDy[x, y];
                coord_offsetsDx[x, y] = coordsDx[x, y] - (double)lowerCoordsDx[x, y];

                lowerCoordsDyMod[x, y] = lowerCoordsDy[x, y] % mapSize;
                lowerCoordsDxMod[x, y] = lowerCoordsDx[x, y] % mapSize;
                upperCoordsDyMod[x, y] = upperCoordsDy[x, y] % mapSize;
                upperCoordsDxMod[x, y] = upperCoordsDx[x, y] % mapSize;

                lowerCoordsDyMod[x, y] = (lowerCoordsDyMod[x, y] < 0) ? lowerCoordsDyMod[x, y] + mapSize : lowerCoordsDyMod[x, y];
                lowerCoordsDxMod[x, y] = (lowerCoordsDxMod[x, y] < 0) ? lowerCoordsDxMod[x, y] + mapSize : lowerCoordsDxMod[x, y];
                upperCoordsDyMod[x, y] = (upperCoordsDyMod[x, y] < 0) ? upperCoordsDyMod[x, y] + mapSize : upperCoordsDyMod[x, y];
                upperCoordsDxMod[x, y] = (upperCoordsDxMod[x, y] < 0) ? upperCoordsDxMod[x, y] + mapSize : upperCoordsDxMod[x, y];

            }
        }


        PrintTabInt(lowerCoordsDy, "lowerCoordsDy");
        PrintTabInt(lowerCoordsDx, "lowerCoordsDx");
        PrintTabInt(upperCoordsDy, "upperCoordsDy");
        PrintTabInt(upperCoordsDx, "upperCoordsDx");

        PrintTabDouble(coord_offsetsDy, "coord_offsetsDy", 0, mapSize);
        PrintTabDouble(coord_offsetsDx, "coord_offsetsDx", 0, mapSize);

        PrintTabInt(lowerCoordsDyMod, "lowerCoordsDyMod");
        PrintTabInt(lowerCoordsDxMod, "lowerCoordsDxMod");
        PrintTabInt(upperCoordsDyMod, "upperCoordsDyMod");
        PrintTabInt(upperCoordsDxMod, "upperCoordsDxMod");


        double[,] result = new double[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                //Debug.Log("lowerCoordsDxMod" + " [" + x + "," + y + "] " + lowerCoordsDxMod[x, y]);
                //Debug.Log("lowerCoordsDyMod" + " [" + x + "," + y + "] " + lowerCoordsDyMod[x, y]);
                result[x, y] = lerp(lerp(a[lowerCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[lowerCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                lerp(a[upperCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[upperCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                coord_offsetsDx[x, y]);
            }
        }

        PrintTabDouble(result, "result", 0, mapSize);

        return result;
    }


    public  double[,] sampleCplx(double[,] a, Complex[,] offset, int mapSize)
    {
        Complex[,] delta = new Complex[mapSize, mapSize];

        double[,] mesggridDy = new double[mapSize, mapSize];
        double[,] mesggridDx = new double[mapSize, mapSize];

        double[,] coordsDy = new double[mapSize, mapSize];
        double[,] coordsDx = new double[mapSize, mapSize];

        int[,] lowerCoordsDy = new int[mapSize, mapSize];
        int[,] lowerCoordsDx = new int[mapSize, mapSize];
        int[,] upperCoordsDy = new int[mapSize, mapSize];
        int[,] upperCoordsDx = new int[mapSize, mapSize];
        double[,] coord_offsetsDy = new double[mapSize, mapSize];
        double[,] coord_offsetsDx = new double[mapSize, mapSize];

        int[,] lowerCoordsDyMod = new int[mapSize, mapSize];
        int[,] lowerCoordsDxMod = new int[mapSize, mapSize];
        int[,] upperCoordsDyMod = new int[mapSize, mapSize];
        int[,] upperCoordsDxMod = new int[mapSize, mapSize];

        double[,] result = new double[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                delta[x, y] = -offset[x, y];

                mesggridDy[x, y] = y;
                mesggridDx[x, y] = x;

                coordsDy[x, y] = mesggridDy[x, y] - (double)delta[x, y].Real;
                coordsDx[x, y] = mesggridDx[x, y] - (double)delta[x, y].Imaginary;

                lowerCoordsDy[x, y] = Mathf.FloorToInt((float)coordsDy[x, y]);
                lowerCoordsDx[x, y] = Mathf.FloorToInt((float)coordsDx[x, y]);
                upperCoordsDy[x, y] = lowerCoordsDy[x, y] + 1;
                upperCoordsDx[x, y] = lowerCoordsDx[x, y] + 1;
                coord_offsetsDy[x, y] = coordsDy[x, y] - (double)lowerCoordsDy[x, y];
                coord_offsetsDx[x, y] = coordsDx[x, y] - (double)lowerCoordsDx[x, y];

                lowerCoordsDyMod[x, y] = lowerCoordsDy[x, y] % mapSize;
                lowerCoordsDxMod[x, y] = lowerCoordsDx[x, y] % mapSize;
                upperCoordsDyMod[x, y] = upperCoordsDy[x, y] % mapSize;
                upperCoordsDxMod[x, y] = upperCoordsDx[x, y] % mapSize;

                lowerCoordsDyMod[x, y] = (lowerCoordsDyMod[x, y] < 0) ? lowerCoordsDyMod[x, y] + mapSize : lowerCoordsDyMod[x, y];
                lowerCoordsDxMod[x, y] = (lowerCoordsDxMod[x, y] < 0) ? lowerCoordsDxMod[x, y] + mapSize : lowerCoordsDxMod[x, y];
                upperCoordsDyMod[x, y] = (upperCoordsDyMod[x, y] < 0) ? upperCoordsDyMod[x, y] + mapSize : upperCoordsDyMod[x, y];
                upperCoordsDxMod[x, y] = (upperCoordsDxMod[x, y] < 0) ? upperCoordsDxMod[x, y] + mapSize : upperCoordsDxMod[x, y];

                result[x, y] = lerp(lerp(a[lowerCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[lowerCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                lerp(a[upperCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[upperCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                coord_offsetsDx[x, y]);
            }
        }
        PrintTabComplex(delta, "-gradient", 0, mapSize);



        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                mesggridDy[x, y] = y;
                mesggridDx[x, y] = x;
            }
        }*/

        PrintTabDouble(mesggridDy, "mesggridDy", 0, mapSize);
        PrintTabDouble(mesggridDx, "mesggridDx", 0, mapSize);



        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                coordsDy[x, y] = mesggridDy[x, y] - (double)delta[x, y].Real;
                coordsDx[x, y] = mesggridDx[x, y] - (double)delta[x, y].Imaginary;
            }
        }*/

        PrintTabDouble(coordsDy, "coordsDy", 0, mapSize);
        PrintTabDouble(coordsDx, "coordsDx", 0, mapSize);



        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                lowerCoordsDy[x, y] = Mathf.FloorToInt((float)coordsDy[x, y]);
                lowerCoordsDx[x, y] = Mathf.FloorToInt((float)coordsDx[x, y]);
                upperCoordsDy[x, y] = lowerCoordsDy[x, y] + 1;
                upperCoordsDx[x, y] = lowerCoordsDx[x, y] + 1;
                coord_offsetsDy[x, y] = coordsDy[x, y] - (double)lowerCoordsDy[x, y];
                coord_offsetsDx[x, y] = coordsDx[x, y] - (double)lowerCoordsDx[x, y];

                lowerCoordsDyMod[x, y] = lowerCoordsDy[x, y] % mapSize;
                lowerCoordsDxMod[x, y] = lowerCoordsDx[x, y] % mapSize;
                upperCoordsDyMod[x, y] = upperCoordsDy[x, y] % mapSize;
                upperCoordsDxMod[x, y] = upperCoordsDx[x, y] % mapSize;

                lowerCoordsDyMod[x, y] = (lowerCoordsDyMod[x, y] < 0) ? lowerCoordsDyMod[x, y] + mapSize : lowerCoordsDyMod[x, y];
                lowerCoordsDxMod[x, y] = (lowerCoordsDxMod[x, y] < 0) ? lowerCoordsDxMod[x, y] + mapSize : lowerCoordsDxMod[x, y];
                upperCoordsDyMod[x, y] = (upperCoordsDyMod[x, y] < 0) ? upperCoordsDyMod[x, y] + mapSize : upperCoordsDyMod[x, y];
                upperCoordsDxMod[x, y] = (upperCoordsDxMod[x, y] < 0) ? upperCoordsDxMod[x, y] + mapSize : upperCoordsDxMod[x, y];

            }
        }*/


        PrintTabInt(lowerCoordsDy, "lowerCoordsDy");
        PrintTabInt(lowerCoordsDx, "lowerCoordsDx");
        PrintTabInt(upperCoordsDy, "upperCoordsDy");
        PrintTabInt(upperCoordsDx, "upperCoordsDx");

        PrintTabDouble(coord_offsetsDy, "coord_offsetsDy", 0, mapSize);
        PrintTabDouble(coord_offsetsDx, "coord_offsetsDx", 0, mapSize);

        PrintTabInt(lowerCoordsDyMod, "lowerCoordsDyMod");
        PrintTabInt(lowerCoordsDxMod, "lowerCoordsDxMod");
        PrintTabInt(upperCoordsDyMod, "upperCoordsDyMod");
        PrintTabInt(upperCoordsDxMod, "upperCoordsDxMod");



        /*for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                //Debug.Log("lowerCoordsDxMod" + " [" + x + "," + y + "] " + lowerCoordsDxMod[x, y]);
                //Debug.Log("lowerCoordsDyMod" + " [" + x + "," + y + "] " + lowerCoordsDyMod[x, y]);
                result[x, y] = lerp(lerp(a[lowerCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[lowerCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                lerp(a[upperCoordsDxMod[x, y], lowerCoordsDyMod[x, y]],
                     a[upperCoordsDxMod[x, y], upperCoordsDyMod[x, y]],
                     coord_offsetsDy[x, y]),
                coord_offsetsDx[x, y]);
            }
        }*/

        PrintTabDouble(result, "result", 0, mapSize);

        return result;
    }




}
