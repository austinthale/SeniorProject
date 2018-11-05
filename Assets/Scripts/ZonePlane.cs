using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonePlane : MonoBehaviour {

    public float length = 0;
    public float width = 0;
    public GameObject zonePrefab;
    public List<GameObject> zoneList;

    private float zoneAmount = 0;
    private Vector3 center = Vector3.zero;

	// Use this for initialization
	void Start () {
        generateFloorZones();
    }

    private void generateFloorZones()
    {
        int k = 0;
        for (int i = 0; i < width; i++)
        {
            float newX = 0;
            float newZ = 0;
            for (int j = 0; j < length; j++)
            {
                newX = center.x - length / 2 + i;
                newZ = center.z - width / 2 + j;
                GameObject temp = Instantiate(zonePrefab, new Vector3(newX, 0 , newZ), Quaternion.identity);
                zoneList.Add(temp);
            } 
            
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
