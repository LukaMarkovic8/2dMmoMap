using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapInteraction : MonoBehaviour
{
    public GameObject dialoguePref;
    public GameObject nameTxt;

    public int buildingsCount;
    public TextMeshProUGUI buildingsCountTXT;

    Vector2 cellOnFingerDown;
    Vector2 cellOnFingerUp;

    public Dictionary<Vector2, GameObject> dialogues = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, GameObject> names = new Dictionary<Vector2, GameObject>();



    private void Start()
    {
        buildingsCount = DataController.dc.mapData.number_of_houses;
        SetNumberOfTilesTxt();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //this prevents tile names from generating if we are zooming
            if (Input.touchCount < 2)
            {
                cellOnFingerDown = CellUnderFinger();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            //this prevents tile names from generating if we are zooming
            if (Input.touchCount < 2)
            {
                cellOnFingerUp = CellUnderFinger();
                //hhis prevents tile names from generating while we are swipeing
                if (cellOnFingerUp == cellOnFingerDown)
                {
                    PickTile();
                }
            }
        }
    }
    /// <summary>
    /// converting the selected position to grid postion
    /// </summary>
    /// <returns></returns>
    Vector2 CellUnderFinger()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //convetring scrren input pos to grid pos
        return new Vector2Int(Mathf.FloorToInt(mousePos.x + 0.5f), Mathf.FloorToInt(mousePos.y + 0.5f));
    }
    /// <summary>
    /// handles tile interaction
    /// </summary>
    void PickTile()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //convetring scrren input pos to grid pos
        Vector2Int key = new Vector2Int(Mathf.FloorToInt(mousePos.x + 0.5f), Mathf.FloorToInt(mousePos.y + 0.5f));
        //checking if we clicked on this tile reacently
        if (!names.ContainsKey(key))
        {
            //Creating the obj
            GameObject name = Instantiate(nameTxt, new Vector2(key.x, key.y), Quaternion.identity);
            name.GetComponent<TextMeshPro>().text = DataController.dc.tileMap[key.x, key.y].type;
        }

        //checking if the tile is destroyable
        if (DataController.dc.tileMap[key.x, key.y].level > 0 && !dialogues.ContainsKey(key))
        {
            //offset vertical position to avoid covering the building
            GameObject dialogue = Instantiate(dialoguePref, new Vector3(key.x, key.y + 1f, -1f), Quaternion.identity);
            dialogue.GetComponent<DestroyBuildingTile>().pos = key;
            dialogues.Add(key, dialogue);
        }
    }

    public void SetNumberOfTilesTxt()
    {
        buildingsCountTXT.text = "Buildings:" + buildingsCount.ToString();
    }
}
