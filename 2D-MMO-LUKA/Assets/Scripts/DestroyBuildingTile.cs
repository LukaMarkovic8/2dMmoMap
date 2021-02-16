using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBuildingTile : MonoBehaviour
{
    public Sprite grassSprite;
    public Vector2Int pos;

    public AudioClip destroyAudio;
    public AudioClip buildingClickAudio;

    MapInteraction mapInteraction;
    ProceduralTerrain mapGenerator;
    AudioSource AudioSource;

    void Awake()
    {
        AudioSource = Camera.main.GetComponent<AudioSource>();
    }
    private void Start()
    {
        mapGenerator = FindObjectOfType<ProceduralTerrain>();
        mapInteraction = FindObjectOfType<MapInteraction>();
        AudioSource.PlayOneShot(buildingClickAudio);
    }

    private void Update()
    {
        if (mapGenerator != null)
        {
            float dist = Vector2.Distance(new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y), new Vector2(transform.position.x, transform.position.y));
            //destroyng the obj if it goes off screen
            if (dist > Camera.main.orthographicSize * 2 + 1)
            {
                mapInteraction.dialogues.Remove(pos);
                Destroy(gameObject);
            }
        }
    }


    //player chose to destroy
    public void DestroyTile()
    {
        //getting the tile we need
        DataController.Tile tile = DataController.dc.tileMap[pos.x, pos.y];

        //changing the tile to grass
        tile.type = "grass";
        tile.name = "EmptyTile";
        tile.level = 0;
        mapGenerator.CurrentTiles[pos].GetComponent<SpriteRenderer>().sprite = grassSprite;


        //changing the number of buildings
        mapInteraction.buildingsCount--;
        mapInteraction.SetNumberOfTilesTxt();

        AudioSource.PlayOneShot(destroyAudio);
        mapInteraction.dialogues.Remove(pos);
        Destroy(gameObject);
    }

    //player chose not to destroy
    public void DontDestroyTile()
    {
        mapInteraction.dialogues.Remove(pos);
        Destroy(gameObject);
    }
}
