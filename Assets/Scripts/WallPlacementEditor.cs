using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WallPlacementEditor : MonoBehaviour
{
    public enum WallType
    {
        None,
        Wall,
        Door,
        Window
    };
    public WallType currentType;
    public bool directionX = false;
    public bool directionZ = false;
    public bool hasWall = false;
    public GameObject wall;
    public bool on = false;
    public GameObject door;
    public bool DoorOn = false;
    public bool isDoorMode = false;

    public GameObject window;
    public bool WindowOn = false;
    public bool isWindowMode = false;

    public void clearAll()
    {
        wall.GetComponent<WallEditor>().clearAll();
        Object.Destroy(wall);
        Object.Destroy(door);
        Object.Destroy(window);
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isDoorMode)
            {
                toggleDoor();
            }
            else if (isWindowMode)
            {
                toggleWindow();
            }
            else
            {
                toggleWall();
            }
        }
        
    }
    public void toggleWall()
    {
        if (!on)
        {
            if (DoorOn)
            {
                turnOffDoor();
            }
            if (WindowOn)
            {
                turnOffWindow();
            }
            wall.gameObject.SetActive(true);
            on = true;
            currentType = WallType.Wall;
        }
        else
        {
            turnOffCameras();
            wall.gameObject.SetActive(false);
            on = false;
            currentType = WallType.None;
        }
    }
    public void turnOffCameras()
    {
        foreach (var cam in wall.GetComponent<WallEditor>().CameraPlacements)
        {
            if (cam.GetComponent<CameraPlacementEditor>().on)
            cam.GetComponent<CameraPlacementEditor>().camOff();
        }
    }
    
    public void toggleDoor()
    {
        if (!DoorOn)
        {
            if (WindowOn)
            {
                turnOffWindow();
            }
            turnOnDoor();
            //currentType = WallType.Door;
        }
        else
        {
            turnOffDoor();
            //currentType = WallType.None;
        }
    }

    public void turnOnDoor()
    {
        if (on)
        {
            toggleWall();
        }
        door.gameObject.SetActive(true);
        DoorOn = true;
        currentType = WallType.Door;
    }

    public void turnOffDoor()
    {
        door.gameObject.SetActive(false);
        DoorOn = false;
        currentType = WallType.None;
    }

    public void toggleWindow()
    {
        if (!WindowOn)
        {
            if (DoorOn)
            {
                turnOffDoor();
            }
            turnOnWindow();
        }
        else
        {
            turnOffWindow();
        }
    }

    public void turnOnWindow()
    {
        if (on)
        {
            toggleWall();
        }
        window.gameObject.SetActive(true);
        WindowOn = true;
        currentType = WallType.Window;
    }

    public void turnOffWindow()
    {
        window.gameObject.SetActive(false);
        WindowOn = false;
        currentType = WallType.None;
    }
}
