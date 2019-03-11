using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEditorManager : MonoBehaviour {
    public GameObject GridManager;
    public List<GameObject> WallPlacements;
    public List<GameObject> Walls = new List<GameObject>();

    public bool wallPlacementActive = true;

    public bool doorMode = false;
    public bool windowMode = false;

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


    public void clearAll()
    {
        foreach(var wp in WallPlacements)
        {
            wp.GetComponent<WallPlacementEditor>().clearAll();
            Object.Destroy(wp);
        }
        WallPlacements.Clear();
    }

    public void toggleWallEditMode()
    {
        if (wallPlacementActive)
        {
            WallOff();
        }
        else
        {
            if (doorMode)
            {
                DoorOff();
            }
            if (windowMode)
            {
                WindowOff();
            }
            WallOn();
        }
    }

    public void toggleDoorEditMode()
    {
        if (!doorMode)
        {
            DoorOn();
        }
        else
        {
            DoorOff();
        }
    }

    public void DoorOn()
    {
        foreach (var wall in WallPlacements)
        {
            wall.GetComponent<WallPlacementEditor>().isDoorMode = true;

        }
        wallPlacementActive = true;
        doorMode = true;
    }

    public void DoorOff()
    {
        foreach (var wall in WallPlacements)
        {
            wall.GetComponent<WallPlacementEditor>().isDoorMode = false;
        }
        wallPlacementActive = true;
        doorMode = false;
    }

    public void toggleWindowEditMode()
    {
        if (!doorMode)
        {
            WindowOn();
        }
        else
        {
            WindowOff();
        }
    }

    public void WindowOn()
    {
        foreach (var wall in WallPlacements)
        {
            wall.GetComponent<WallPlacementEditor>().isWindowMode = true;

        }
        wallPlacementActive = true;
        windowMode = true;
    }

    public void WindowOff()
    {
        foreach (var wall in WallPlacements)
        {
            wall.GetComponent<WallPlacementEditor>().isWindowMode = false;
        }
        wallPlacementActive = true;
        windowMode = false;
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
