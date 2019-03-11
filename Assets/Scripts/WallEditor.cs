using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEditor : MonoBehaviour {

    public GameObject placementObject;
    public List<GameObject> CameraPlacements;

    public void clearAll()
    {
        foreach (var cam in CameraPlacements)
        {
            Object.Destroy(cam);
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
