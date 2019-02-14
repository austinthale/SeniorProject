using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour
{
    public GameObject zonePrefab;
    public GameObject WallPlacementPrefab;
    public int gridHeight = 8;  // number of cubes on an edge
    public int gridWidth = 8;
    Vector3 v3Center = Vector3.zero;
    public GameObject gridStart;

    public GameObject floorCube;

    public GameObject wallX;
    public GameObject wallZ;
    public GameObject wallPlacementX;
    public GameObject wallPlacementZ;

    public GameObject WallPlacementParentObject;
    public GameObject WallParent;
    public GameObject zoneList;

    public GameObject GeneralEditorManager;

    //public GameObject[,] argo = new GameObject[gridHeight, gridWidth];
    public List<GameObject> gridList;
    public List<GameObject> WallPlacementList;
    public List<GameObject> walls;
    public List<GameObject> triggers;
    // Use this for initialization
    void Start()
    {
        genZones();
        //genWallPlacement();
    }

    private void gridReset()
    {

        for (int i = gridList.Count - 1; i < -1; i--)
        {
            GameObject temp = gridList[i];
            gridList.Remove(gridList[i]);
            Destroy(temp);
        }
        for (int i = WallPlacementList.Count - 1; i < -1; i--)
        {
            GameObject temp = WallPlacementList[i];
            WallPlacementList.Remove(WallPlacementList[i]);
            Destroy(temp);
        }
        for (int i = triggers.Count - 1; i < -1; i--)
        {
            GameObject temp = triggers[i];
            triggers.Remove(triggers[i]);
            Destroy(temp);
        }
    }

    public void genZones()
    {
        gridReset();
        if (gridWidth % 2 != 0)
        {
            gridWidth++;
        }
        if (gridHeight % 2 != 0)
        {
            gridHeight++;
        }
        if (gridWidth != gridHeight)
        {
            gridWidth = gridHeight;
        }
        transform.position = v3Center;
        v3Center = transform.position;
        for (var i = 0; i < gridHeight; i++)
        {
            for (var j = 0; j < gridWidth; j++)
            {
                float x = ((float)(v3Center.x - gridHeight / 2.0 + i)) * zonePrefab.transform.localScale.x;
                float z = ((float)(v3Center.z - gridWidth / 2.0 + j)) * zonePrefab.transform.localScale.z;
                GameObject temp = Instantiate(zonePrefab, new Vector3(x, 0, z), Quaternion.identity);
                temp.transform.parent = zoneList.transform;
                gridList.Add(temp);
            }
        }
        for (int k = 0; k < gridList.Count; k++)
        {
            gridList[k].name = "Zone " + (k + 1);
            gridList[k].GetComponent<ZoneAnalysis>().ZoneName = "Zone " + (k + 1);
            //gridList[k].GetComponent<ZoneAnalysis>().trigger.transform.parent = null;
        }
        genWallPlacement();
        floorGen();
    }

    void genWallPlacement()
    {
        if (gridWidth % 2 != 0)
        {
            gridWidth++;
        }
        if (gridHeight % 2 != 0)
        {
            gridHeight++;
        }
        transform.position = v3Center;
        v3Center = transform.position;
        //v3Center.z += WallPlacementPrefab.transform.localScale.z / 2;
        //v3Center.x += WallPlacementPrefab.transform.localScale.x / 2;
        for (var i = 0; i < (gridHeight*2) + 1; i++)
        {
            if (i % 2 == 0)
            {
                for (var j = 0; j < (gridWidth); j++)
                {
                    float x = ((((float)(v3Center.x - gridHeight + i)) * zonePrefab.transform.localScale.x / 2)) - (zonePrefab.transform.localScale.x / 2);
                    float z = (((float)(v3Center.z - gridWidth + j)) * zonePrefab.transform.localScale.z) + zonePrefab.transform.localScale.z * (gridWidth / 2);
                    float y = (wallZ.transform.localScale.y / 2);
                    GameObject temp = Instantiate(wallPlacementZ, new Vector3(x, 4.8f, z), Quaternion.identity);
                    temp.transform.parent = WallPlacementParentObject.transform;
                    GameObject w = temp.GetComponent<WallPlacementEditor>().wall;
                    walls.Add(w);
                    assignGeneralManager(w);
                    w.transform.parent = WallParent.transform;
                    //temp.GetComponent<WallPlacementEditor>().wall = Instantiate(wallZ, new Vector3(x, y, z), Quaternion.identity);
                    WallPlacementList.Add(temp);
                }
            }
            else
            {
            for (var j = 0; j < (gridWidth + 1); j++)
            {
                    float x = ((((float)(v3Center.x - gridHeight / 2.0 + i)) * zonePrefab.transform.localScale.x / 2)) - ((0.5f * ((gridWidth / 2) + 1)) * zonePrefab.transform.localScale.x);
                    float z = (((float)(v3Center.z - gridWidth / 2.0 + j)) * zonePrefab.transform.localScale.z) - (zonePrefab.transform.localScale.x / 2);
                    float y = (wallX.transform.localScale.y / 2);
                    GameObject temp = Instantiate(wallPlacementX, new Vector3(x, 4.8f, z), Quaternion.identity);
                    temp.transform.Rotate(0, 90, 0);
                    temp.transform.parent = WallPlacementParentObject.transform;
                    GameObject w = temp.GetComponent<WallPlacementEditor>().wall;
                    walls.Add(w);
                    w.transform.parent = WallParent.transform;
                    assignGeneralManager(w);
                    //GameObject w = Instantiate(wallX, new Vector3(x, y, z), Quaternion.identity);
                    //temp.GetComponent<WallPlacementEditor>().wall = w;
                    WallPlacementList.Add(temp);
                }
        }
        }
        /* for (int k = 0; k < gridList.Count; k++)
         {
             gridList[k].name = "Zone " + (k + 1);
             gridList[k].GetComponent<ZoneAnalysis>().ZoneName = "Zone " + (k + 1);
             gridList[k].GetComponent<ZoneAnalysis>().trigger.transform.parent = null;
         }*/
        turnWallsOff();
    }

    public void assignGeneralManager(GameObject wa)
    {
        foreach (var wall in wa.GetComponent<WallEditor>().CameraPlacements)
        {
            wall.GetComponent<CameraPlacementEditor>().GeneralEditorManager = GeneralEditorManager;
        }
    }

    public void floorGen()
    {
        GameObject floor = Instantiate(floorCube, new Vector3(-(zonePrefab.transform.localScale.x / 2), -0.5f, -(zonePrefab.transform.localScale.z / 2)), Quaternion.identity);
        floor.transform.localScale += new Vector3((zonePrefab.transform.localScale.x * gridWidth), 0, (zonePrefab.transform.localScale.z * gridWidth));

    }

    public void turnWallsOff()
    {
        foreach (var item in WallPlacementList)
        {
            item.GetComponent<WallPlacementEditor>().wall.gameObject.SetActive(false);
        }
    }

}
