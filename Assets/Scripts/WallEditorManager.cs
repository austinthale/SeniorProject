using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEditorManager : MonoBehaviour {
    public GameObject GridManager;
    public List<GameObject> WallPlacements;
    public List<GameObject> Walls = new List<GameObject>();

    public bool wallPlacementActive = true;

	// Use this for initialization
	void Start () {
        WallPlacements = GridManager.GetComponent<GenerateGrid>().WallPlacementList;
        Walls = GridManager.GetComponent<GenerateGrid>().walls;
    }

    private void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            toggleWallEditMode();
        }
    }

    public void toggleWallEditMode()
    {
        if (wallPlacementActive)
        {
            WallOff();
        }
        else
        {
            WallOn();
        }
    }

    public void WallOn()
    {
        foreach (var wall in WallPlacements)
        {
            wall.gameObject.SetActive(true);
        }
        wallPlacementActive = true;
    }

    public void WallOff()
    {
        foreach (var wall in WallPlacements)
        {
            wall.gameObject.SetActive(false);
        }
        wallPlacementActive = false;
    }


}
