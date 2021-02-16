using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public float scale = 20f;

    public float offsetY = 1f;
    public float offsetX = 1f;

    float groundTileTypes = 5;

    // Start is called before the first frame update
    void Awake()
    {
        //random offset makes the map random every time we generate it
        offsetY = Random.Range(0, 999f);
        offsetX = Random.Range(0, 999f);
    }

    /// <summary>
    /// this method returns a 2d array of ints, where each int represents a terrain tile type
    /// by using perlin noise we get a nice looking terrain
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public int[,] GanerateMap(int width, int height)
    {
        int[,] tiles = new int[width, height];

        //
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                float type = CreateTile(width, height, x, y);
                //clamping the value because i was getting values below zero and above 4, so i just decided to clamp it. Something I would come back to    
                int a = Mathf.Clamp((int)(groundTileTypes * type), 0, 4);
                //setting the tile
                tiles[x, y] = a;
            }
        }
        return tiles;
    }

    //creating the perin value for the tile
    float CreateTile(int width, int height, int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }

}
