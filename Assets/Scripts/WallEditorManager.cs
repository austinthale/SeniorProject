using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEditorManager : MonoBehaviour {
    public GameObject GridManager;
    public List<GameObject> WallPlacements;
    public List<GameObject> Walls = new List<GameObject>();

	// Use this for initialization
	void Start () {
        WallPlacements = GridManager.GetComponent<GenerateGrid>().WallPlacementList;
	}

    
}
