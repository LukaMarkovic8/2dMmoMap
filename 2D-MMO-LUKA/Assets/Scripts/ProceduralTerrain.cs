using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrain : MonoBehaviour
{
    public GameObject tilesHolder;
    public GameObject[] tileTypes;
    public CameraController cameraController;

    public Dictionary<Vector2Int, GameObject> CurrentTiles = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, List<DataController.Tile>> currentChunks = new Dictionary<Vector2Int, List<DataController.Tile>>();
    int lastZoomValue;

    Vector2Int myChunk = new Vector2Int();
    // Start is called before the first frame update
    void Start()
    {
        lastZoomValue = (int)cameraController.desiredCameraSize;
        transform.position = new Vector3(DataController.dc.firstHousePos.x, DataController.dc.firstHousePos.y, 0);
        myChunk = FindMyChunkPos(gameObject);
        StartCoroutine(NewSurrounding(myChunk));
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int Chunk = FindMyChunkPos(gameObject);
        int zoomDiff = Mathf.Abs(lastZoomValue - (int)cameraController.desiredCameraSize);

        //checking if we changed the chunk we are in or the zoom value is changed
        if (Chunk != myChunk || zoomDiff > 0)
        {
            lastZoomValue = (int)cameraController.desiredCameraSize;
            myChunk = Chunk;
            StartCoroutine(NewSurrounding(myChunk));
        }
    }


    Vector2Int FindMyChunkPos(GameObject pGameObject)
    {
        return new Vector2Int((int)pGameObject.transform.position.x / DataController.dc.chunkSize, (int)pGameObject.transform.position.y / DataController.dc.chunkSize);
    }

    void CreateTile(DataController.Tile pTile)
    {
        GameObject tileGO = Instantiate(GetTileByType(pTile.type), new Vector2(pTile.position.x, pTile.position.y), Quaternion.identity, tilesHolder.transform);
        CurrentTiles.Add(pTile.position, tileGO);
    }

    //casting the tile type to tile prefab
    GameObject GetTileByType(string pType)
    {
        switch (pType)
        {
            case "grass":
                return tileTypes[0];
            case "house1":
                return tileTypes[1];
            case "house2":
                return tileTypes[2];
            case "sand":
                return tileTypes[3];
            case "trees1":
                return tileTypes[4];
            case "trees2":
                return tileTypes[5];
            case "water":
                return tileTypes[6];

            default:
                Debug.LogError("WRONG NAME");
                return new GameObject();
                break;
        }
    }

    /// <summary>
    /// chooses new chunks depending on the new position
    /// </summary>
    /// <param name="pMyChunk"></param>
    /// <param name="pRadius"></param>
    /// <returns></returns>
    Dictionary<Vector2Int, List<DataController.Tile>> newMapChunks(Vector2Int pMyChunk, int pRadius)
    {
        int radiusX = pRadius;
        int radiusY = pRadius;
        Dictionary<Vector2Int, List<DataController.Tile>> newChuks = new Dictionary<Vector2Int, List<DataController.Tile>>();
        for (int i = -radiusX; i <= radiusX; i++)
        {
            for (int j = -radiusY; j <= radiusY; j++)
            {
                Vector2Int chunkPos = new Vector2Int(pMyChunk.x + i, pMyChunk.y + j);
                //checking if the chunk exists
                if (pMyChunk.x + i >= 0 && pMyChunk.x + i < DataController.dc.mapData.map_width / DataController.dc.chunkSize && pMyChunk.y + j >= 0 && pMyChunk.y + j < DataController.dc.mapData.map_width / DataController.dc.chunkSize)
                {
                    newChuks.Add(chunkPos, DataController.dc.chunkedMap[pMyChunk.x + i, pMyChunk.y + j]);
                }
            }
        }
        return newChuks;
    }


    /// <summary>
    /// this method takes care of crating new tiles and destroying old ones
    /// </summary>
    /// <param name="pMyChunk"></param>
    /// <returns></returns>

    IEnumerator NewSurrounding(Vector2Int pMyChunk)
    {
        Dictionary<Vector2Int, List<DataController.Tile>> newChuks = newMapChunks(pMyChunk, (int)(Camera.main.orthographicSize / 2 + 2));
        List<Vector2Int> obsoleteChunks = new List<Vector2Int>();
        //AddingNewChnuks
        foreach (var chunk in newChuks.Keys)
        {
            if (!currentChunks.ContainsKey(chunk))
            {
                currentChunks.Add(chunk, DataController.dc.chunkedMap[chunk.x, chunk.y]);
                foreach (var tile in DataController.dc.chunkedMap[chunk.x, chunk.y])
                {
                    if (!CurrentTiles.ContainsKey(tile.position))
                    {

                        CreateTile(tile);
                    }
                }
            }
        }

        //finding obsolete chunks
        foreach (var chunk in currentChunks.Keys)
        {
            if (!newChuks.ContainsKey(chunk))
            {
                obsoleteChunks.Add(chunk);
            }
        }

        yield return new WaitForEndOfFrame();
        //removing obsolete chnks and destroynig obsolete tiles
        foreach (var obsoleteChunk in obsoleteChunks)
        {
            currentChunks.Remove(obsoleteChunk);
            foreach (var tile in DataController.dc.chunkedMap[obsoleteChunk.x, obsoleteChunk.y])
            {
                if (CurrentTiles.ContainsKey(tile.position))
                {
                    Destroy(CurrentTiles[tile.position]);
                    CurrentTiles.Remove(tile.position);
                }
            }

        }
    }

}
