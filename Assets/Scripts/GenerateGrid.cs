using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour {
    public GameObject goPrefab;
    public int gridHeight = 8;  // number of cubes on an edge
    public int gridWidth = 8;
    Vector3 v3Center = Vector3.zero;

    public GameObject parentObj;

    //public GameObject[,] argo = new GameObject[gridHeight, gridWidth];
    public List<GameObject> gridList;
    // Use this for initialization
    void Start () {
        for (var i = 0; i < gridHeight; i++)
        {
            for (var j = 0; j < gridWidth; j++)
            {
                float x = (float)(v3Center.x - gridHeight / 2.0 + i) * goPrefab.transform.localScale.x;
                float z = (float)(v3Center.z - gridWidth / 2.0 + j) * goPrefab.transform.localScale.z;
                GameObject temp = Instantiate(goPrefab, new Vector3(x, 0, z), Quaternion.identity);
                temp.transform.parent =this.gameObject.transform;
                gridList.Add(temp);
            }
        }
        for (int k = 0; k < gridList.Count; k++)
        {
            gridList[k].name = "Zone " + (k + 1);
            gridList[k].GetComponent<ZoneAnalysis>().ZoneName = "Zone " + (k + 1);
        }
    }
}
