using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataController : MonoBehaviour
{
    public static DataController dc;
    string jsonURL = "https://gist.githubusercontent.com/anonymous/63de7fecde7289804f95619c9d20c7ad/raw/a6e24694cfbfef42fbb018283b5e570173a2e816/map.json";
    public MapData mapData;

    //all of the tiles
    public Tile[,] tileMap = new Tile[1, 1];
    //chunked map used in procedural generation for better perfomance
    public List<Tile>[,] chunkedMap = new List<Tile>[1, 1];

    public int chunkSize = 16;
    public Vector2 firstHousePos;

    PerlinNoise perlin;
    bool done = false;

    //used to parse json
    public class MapData
    {
        public int map_width;
        public int map_height;
        public int number_of_houses;
        public Tile[] tiles;
    }


    //Class for holding the tile information
    [Serializable]
    public class Tile
    {
        public string type = "";
        public string name = "";
        public int level;
        public Vector2Int position;
    }

    void Awake()
    {
        dc = this;
        DontDestroyOnLoad(this);
        //
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        perlin = GetComponent<PerlinNoise>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetData());
    }

    /// <summary>
    /// Getting the data from the URL
    /// </summary>
    /// <returns></returns>
    IEnumerator GetData()
    {
        Debug.Log("GettingData");
        WWW webData = new WWW(jsonURL);
        //waiting
        yield return webData;

        if (webData.error != null)
        {
            Debug.LogError("WEB ERRR");
        }
        //parisng the data into the desired object form
        mapData = JsonUtility.FromJson<MapData>(webData.text);

        while (mapData.tiles.Length < 2)
        {
            Debug.Log("wait for parse");
            yield return new WaitForEndOfFrame();

        }
        // CreateMap
        StartCoroutine(GenerateMap(mapData.map_height, mapData.map_width));
        while (!done)
        {
            Debug.Log("waiting for map");
            yield return new WaitForEndOfFrame();
        }

        LoadScene();
        StopCoroutine(GetData());
    }

    void LoadScene()
    {
        Debug.Log("DataLoaded");
        SceneManager.LoadSceneAsync("Map");
    }


    IEnumerator GenerateMap(int pMapHeight, int pMapWidth)
    {
        //creating a periln values map
        int[,] map = perlin.GanerateMap(pMapWidth, pMapHeight);
        //initializing tilemap to desired size
        tileMap = new Tile[pMapHeight, pMapWidth];
        chunkedMap = new List<Tile>[pMapHeight / chunkSize, pMapWidth / chunkSize];

        //we have to initialize the array before using
        //without this step there were no lists to add the tiles later
        for (int x = 0; x < pMapHeight / chunkSize; x++)
        {
            for (int y = 0; y < pMapWidth / chunkSize; y++)
            {
                chunkedMap[x, y] = new List<Tile>();
            }
        }
        //we will use this list for generateing houses   
        List<Vector2Int> buildableTiles = new List<Vector2Int>();

        yield return new WaitForEndOfFrame();
        for (int i = 0; i < pMapWidth; i++)
        {
            for (int j = 0; j < pMapHeight; j++)
            {

                Tile newTile;
                //casting perlin values to tile types
                if (map[i, j] == 0)
                {
                    newTile = new Tile()
                    { type = "water", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                }
                else if (map[i, j] == 1)
                {
                    newTile = new Tile()
                    { type = "sand", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                }
                else if (map[i, j] == 2)
                {
                    newTile = new Tile()
                    { type = "grass", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                    buildableTiles.Add(new Vector2Int(i, j));
                }
                else if (map[i, j] == 3)
                {
                    //randomizing
                    int tmp = UnityEngine.Random.Range(0, 2);
                    if (tmp == 0)
                    {
                        newTile = new Tile()
                        { type = "trees1", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                    }
                    else
                    {
                        newTile = new Tile()
                        { type = "trees2", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                    }
                    buildableTiles.Add(new Vector2Int(i, j));
                }
                else if (map[i, j] == 4)
                {
                    newTile = new Tile()
                    { type = "trees2", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
                }
                else
                {
                    Debug.LogError("DIDNT FIND TILE TYPE FOR NUMBER:" + map[i, j]);
                    newTile = new Tile();
                }

                //adding to tile Map
                tileMap[i, j] = newTile;

                //adding to the chunked tile map
                int x = Mathf.FloorToInt((float)i / chunkSize);
                int y = Mathf.FloorToInt((float)j / chunkSize);
                chunkedMap[x, y].Add(newTile);
            }
        }

        Dictionary<Vector2Int, Tile> houses = new Dictionary<Vector2Int, Tile>();

        for (int i = 0; i < mapData.number_of_houses; i++)
        {

            int randTile = UnityEngine.Random.Range(0, buildableTiles.Count);
            Vector2Int pos = buildableTiles[randTile];
            int offset = 20;
            //Making sure not to add to the same tile multiple times  or is to close to edge of the map
            while (houses.ContainsKey(pos) || (pos.x < offset || pos.x > mapData.map_width - offset) || (pos.y < offset || pos.y > mapData.map_height - offset))
            {
                randTile = UnityEngine.Random.Range(0, buildableTiles.Count);
                pos = buildableTiles[randTile];
            }

            Tile tile;
            //randomizing tyle type
            int tmp = UnityEngine.Random.Range(0, 2);
            if (tmp == 0)
            {
                tile = new Tile()
                { type = "house2", name = "Storage", level = 1, position = pos };
            }
            else
            {
                tile = new Tile()
                { type = "house1", name = "Barrack", level = 2, position = pos };
            }

            houses.Add(pos, tile);
            //changing the tile on the tilemap    
            tileMap[pos.x, pos.y] = tile;

            //adding to the chunked tile map
            int x = Mathf.FloorToInt((float)pos.x / chunkSize);
            int y = Mathf.FloorToInt((float)pos.y / chunkSize);

            foreach (var item in chunkedMap[x, y])
            {
                if (item.position == new Vector2Int(pos.x, pos.y))
                {
                    //removing old tile from the list of tiles in this chunk 
                    chunkedMap[x, y].Remove(item);
                    break;
                }
            }

            //adding new tile to the chunked tile map
            chunkedMap[x, y].Add(tile);

            //saving the position a house so we can set the camera on map scene to look at a house on starting the scene
            if (i == 0)
            {
                firstHousePos = pos;
            }
        }
        //signaling the map is done
        done = true;
    }

    #region obsoleteCode
    //void GenerateChunksS()
    //{
    //    /*int numberOfChunks = mapData.map_height * mapData.map_width / chunkSize / chunkSize;
    //    Vector2[,] mapChunksNew = new Vector2[numberOfChunks / numberOfChunks, numberOfChunks / numberOfChunks];
    //    */


    //    foreach (var item in Cells)
    //    {
    //        int x = Mathf.FloorToInt((float)item.Key.x / chunkSize);
    //        int y = Mathf.FloorToInt((float)item.Key.y / chunkSize);
    //        Vector2 chunk = new Vector2(x, y);
    //        //  Vector2[]
    //        if (mapChunks.ContainsKey(chunk))
    //        {
    //            mapChunks[chunk].Add(new Vector2(item.Key.x, item.Key.y));
    //        }
    //        else
    //        {
    //            mapChunks.Add(new Vector2(x, y), new List<Vector2>() { new Vector2(item.Key.x, item.Key.y) });
    //        }
    //    }

    //    Debug.Log("CHUNKS::::" + mapChunks.Keys.Count);
    //    done = true;

    //}
    ///*
    //    IEnumerator GenerateMap(int pMapHeight, int pMapWidth)
    //    {
    //        //int[,] map = perlin.GanerateMap();
    //        Tile[,] tileMap = new Tile[pMapHeight, pMapWidth];


    //        Debug.Log("Total tiles " + map.Length);

    //        List<Vector2Int> buildableTiles = new List<Vector2Int>();
    //        yield return new WaitForEndOfFrame();
    //        for (int i = 0; i < mapData.map_height; i++)
    //        {
    //            for (int j = 0; j < mapData.map_width; j++)
    //            {
    //                Tile newTile;
    //                //  Debug.LogError(i + "      " + j);
    //                if (map[i, j] == 0)
    //                {
    //                    newTile = new Tile()
    //                    { type = "water", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                }
    //                else if (map[i, j] == 1)
    //                {
    //                    newTile = new Tile()
    //                    { type = "sand", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                }
    //                else if (map[i, j] == 2)
    //                {
    //                    newTile = new Tile()
    //                    { type = "grass", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                    buildableTiles.Add(new Vector2Int(i, j));
    //                }
    //                else if (map[i, j] == 3)
    //                {
    //                    if ((i + j) % 2 == 0)
    //                    {
    //                        newTile = new Tile()
    //                        { type = "trees1", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                    }
    //                    else
    //                    {
    //                        newTile = new Tile()
    //                        { type = "trees2", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                    }
    //                    buildableTiles.Add(new Vector2Int(i, j));
    //                }
    //                else if (map[i, j] == 4)
    //                {
    //                    newTile = new Tile()
    //                    { type = "trees2", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                }
    //                else
    //                {
    //                    Debug.LogError("DIDNT FIND TYLE FOR NUMBER:" + map[i, j]);
    //                    newTile = new Tile();
    //                }

    //                tileMap[i, j] = newTile;

    //                Cells.Add(new Vector2Int(i, j), newTile);
    //            }

    //        }

    //        Dictionary<Vector2Int, Tile> houses = new Dictionary<Vector2Int, Tile>();

    //        for (int i = 0; i < mapData.number_of_houses; i++)
    //        {
    //            int randTile = UnityEngine.Random.Range(0, buildableTiles.Count);
    //            Vector2Int pos = buildableTiles[randTile];
    //            int offset = 20;
    //            //Making sure not to add to the same tile multiple times 
    //            while (houses.ContainsKey(pos) || (pos.x < offset || pos.x > mapData.map_width - offset) || (pos.y < offset || pos.y > mapData.map_height - offset))
    //            {
    //                randTile = UnityEngine.Random.Range(0, buildableTiles.Count);
    //                pos = buildableTiles[randTile];
    //            }

    //            Tile tile;
    //            if (i % 2 == 0)
    //            {
    //                tile = new Tile()
    //                { type = "house2", name = "Storage", level = 1, position = pos };
    //            }
    //            else
    //            {
    //                tile = new Tile()
    //                { type = "house1", name = "Barrack", level = 2, position = pos };
    //            }
    //            houses.Add(pos, tile);
    //            Cells[pos] = tile;
    //            tileMap[pos.x, pos.y] = tile;

    //            if (i == 0)
    //            {
    //                firstHousePos = pos;
    //            }
    //        }
    //        GenerateChunks();

    //        Debug.Log("22222222 number of cells " + Cells.Keys.Count);
    //        Debug.Log("22222222 number of houses " + houses.Keys.Count);
    //    }
    //*/
    //void GenerateChunks()
    //{
    //    int numberOfChunks = mapData.map_height * mapData.map_width / chunkSize / chunkSize;
    //    Vector2[,] mapChunksNew = new Vector2[numberOfChunks / numberOfChunks, numberOfChunks / numberOfChunks];



    //    foreach (var item in Cells)
    //    {
    //        int x = Mathf.FloorToInt((float)item.Key.x / chunkSize);
    //        int y = Mathf.FloorToInt((float)item.Key.y / chunkSize);
    //        Vector2 chunk = new Vector2(x, y);
    //        //  Vector2[]
    //        if (mapChunks.ContainsKey(chunk))
    //        {
    //            mapChunks[chunk].Add(new Vector2(item.Key.x, item.Key.y));
    //        }
    //        else
    //        {
    //            mapChunks.Add(new Vector2(x, y), new List<Vector2>() { new Vector2(item.Key.x, item.Key.y) });
    //        }
    //    }

    //    Debug.Log("CHUNKS::::" + mapChunks.Keys.Count);
    //    done = true;

    //}
    ///// <summary>
    ///// This method generates the types and position of the tiles
    ///// Randomly putting the buildings anywhere on the map, then generating each rim after another by the prioroty of instancing
    ///// </summary>
    ///*   void CreateMap()
    //   {


    //       Dictionary<Vector2Int, Tile> CurrentBuildings = new Dictionary<Vector2Int, Tile>();
    //       //GENERATING BUILDINGS
    //       for (int i = 0; i < mapData.number_of_houses / 2; i++)
    //       {
    //           Vector2Int pos = new Vector2Int(UnityEngine.Random.Range(12, 500), UnityEngine.Random.Range(12, 500));
    //           while (CurrentBuildings.ContainsKey(pos))
    //           {
    //               pos = new Vector2Int(UnityEngine.Random.Range(12, 500), UnityEngine.Random.Range(12, 500));
    //           }
    //           Tile barrack = new Tile()
    //           { type = "house1", name = "Barrack", level = 2, position = pos };

    //           CurrentBuildings.Add(pos, barrack);
    //           Cells.Add(pos, barrack);
    //       }
    //       //GENERATING BUILDINGS
    //       for (int i = 0; i < mapData.number_of_houses / 2; i++)
    //       {
    //           Vector2Int pos = new Vector2Int(UnityEngine.Random.Range(12, 500), UnityEngine.Random.Range(12, 500));
    //           while (CurrentBuildings.ContainsKey(pos))
    //           {
    //               pos = new Vector2Int(UnityEngine.Random.Range(12, 500), UnityEngine.Random.Range(12, 500));
    //           }
    //           Tile barrack = new Tile()
    //           { type = "house2", name = "Storage", level = 1, position = pos };

    //           CurrentBuildings.Add(pos, barrack);
    //           Cells.Add(pos, barrack);
    //           //SAVING A POSITION OF A BUILDING TO SET THE CAMERA TO LOOK AT A BUILDING ON STARTING THE MAP SCENE
    //           if (i == 0)
    //           {
    //               firstHousePos = pos;
    //           }
    //       }


    //       //GENERATING GRASS
    //       Dictionary<Vector2Int, Tile> grassRim = new Dictionary<Vector2Int, Tile>();
    //       foreach (var keys in CurrentBuildings)
    //       {
    //           int radius = 2;
    //           Vector2Int pos = new Vector2Int(keys.Key.x, keys.Key.y);
    //           for (int x = -radius; x <= radius; x++)
    //           {
    //               for (int y = -radius; y <= radius; y++)
    //               {
    //                   Vector2Int aroundPos = pos + new Vector2Int(x, y);
    //                   int diff = (int)(Mathf.Abs(aroundPos.x - pos.x) + Mathf.Abs(aroundPos.y - pos.y));

    //                   if (diff > radius)
    //                   { }
    //                   else if (!Cells.ContainsKey(aroundPos) && aroundPos.x >= 0 && aroundPos.x <= mapData.map_width && aroundPos.y > 0 && aroundPos.y <= mapData.map_height)
    //                   {
    //                       Tile grass = new Tile()
    //                       { type = "grass", name = "Empty Tile", level = 0, position = aroundPos };
    //                       grassRim.Add(aroundPos, grass);
    //                       Cells.Add(aroundPos, grass);
    //                   }
    //               }
    //           }
    //       }

    //       Dictionary<Vector2Int, Tile> thinForestRim = new Dictionary<Vector2Int, Tile>();

    //       //GENERATING THIN FOREST RIM
    //       foreach (var keys in grassRim)
    //       {
    //           int radius = 3;
    //           Vector2Int pos = new Vector2Int(keys.Key.x, keys.Key.y);
    //           for (int x = -radius; x <= radius; x++)
    //           {
    //               for (int y = -radius; y <= radius; y++)
    //               {
    //                   Vector2Int aroundPos = pos + new Vector2Int(x, y);
    //                   int diff = (int)(Mathf.Abs(aroundPos.x - pos.x) + Mathf.Abs(aroundPos.y - pos.y));

    //                   if (diff > radius)
    //                   { }
    //                   else if (!Cells.ContainsKey(aroundPos) && aroundPos.x > 0 && aroundPos.x <= mapData.map_width && aroundPos.y >= 0 && aroundPos.y <= mapData.map_height)
    //                   {
    //                       //randomizing the type of trees
    //                       int a = UnityEngine.Random.Range(0, 2);
    //                       string tmpType = "";
    //                       if (a == 0)
    //                       {
    //                           tmpType = "trees1";
    //                       }
    //                       else
    //                       {
    //                           tmpType = "trees2";
    //                       }

    //                       Tile thinForest = new Tile()
    //                       { type = tmpType, name = "Empty Tile", level = 0, position = aroundPos };

    //                       thinForestRim.Add(aroundPos, thinForest);
    //                       Cells.Add(aroundPos, thinForest);
    //                   }
    //               }
    //           }
    //       }

    //       Dictionary<Vector2Int, Tile> thickForestRim = new Dictionary<Vector2Int, Tile>();
    //       Dictionary<Vector2Int, Tile> thickForestOuterRim = new Dictionary<Vector2Int, Tile>();

    //       //GENERATING THICK FOREST
    //       foreach (var keys in thinForestRim)
    //       {
    //           int radius = 20;

    //           Vector2Int pos = new Vector2Int(keys.Key.x, keys.Key.y);
    //           for (int x = -radius; x <= radius; x++)
    //           {
    //               for (int y = -radius; y <= radius; y++)
    //               {
    //                   Vector2Int aroundPos = pos + new Vector2Int(x, y);

    //                   int diff = (int)(Mathf.Abs(aroundPos.x - pos.x) + Mathf.Abs(aroundPos.y - pos.y));

    //                   if (diff > radius)
    //                   { }
    //                   else if (!Cells.ContainsKey(aroundPos) && aroundPos.x >= 0 && aroundPos.x <= mapData.map_width && aroundPos.y >= 0 && aroundPos.y <= mapData.map_height)
    //                   {
    //                       Tile thickForest = new Tile()
    //                       { type = "trees2", name = "Empty Tile", level = 0, position = aroundPos };
    //                       thickForestRim.Add(aroundPos, thickForest);
    //                       Cells.Add(aroundPos, thickForest);

    //                       //SAVING THE OUTER RIM AS THE TILES ARE GENERATED 
    //                       if (diff == radius)
    //                       {
    //                           thickForestOuterRim.Add(aroundPos, thickForest);

    //                       }
    //                   }
    //               }
    //           }

    //       }


    //       //GENERATING SAND TILES AROUND THE FOREST
    //       foreach (var keys in thickForestOuterRim)
    //       {
    //           int radius = 1;
    //           for (int x = -radius; x <= radius; x++)
    //           {
    //               for (int y = -radius; y <= radius; y++)
    //               {
    //                   Vector2Int aroundPos = new Vector2Int(keys.Key.x + x, keys.Key.y + y);

    //                   if (!Cells.ContainsKey(aroundPos) && aroundPos.x >= 0 && aroundPos.x <= mapData.map_width && aroundPos.y > +0 && aroundPos.y <= mapData.map_height)
    //                   {
    //                       Tile sand = new Tile()
    //                       { type = "sand", name = "Empty Tile", level = 0, position = aroundPos };

    //                       Cells.Add(aroundPos, sand);
    //                   }

    //               }
    //           }
    //       }

    //       //generating water tiles on the ramaining area
    //       for (int i = 0; i <= mapData.map_height; i++)
    //       {
    //           for (int j = 0; j <= mapData.map_width; j++)
    //           {
    //               if (!Cells.ContainsKey(new Vector2Int(i, j)))
    //               {
    //                   Tile water = new Tile()
    //                   { type = "water", name = "Empty Tile", level = 0, position = new Vector2Int(i, j) };
    //                   Cells.Add(new Vector2Int(i, j), water);
    //                   //count++;
    //               }
    //           }
    //       }
    //       GenerateChunks();

    //       done = true;
    //   }*/

    /// <summary>
    /// Deviding the map into smaller chunks for better procedural generation
    /// this enables us to iterate faster thru the map and find tiles close by
    /// </summary>  

    #endregion
}
