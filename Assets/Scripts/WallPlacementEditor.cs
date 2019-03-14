using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WallPlacementEditor : MonoBehaviour
{

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
        }
        else
        {
            turnOffCameras();
            wall.gameObject.SetActive(false);
            on = false;
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
        }
        else
        {
            turnOffDoor();
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
    }

    public void turnOffDoor()
    {
        door.gameObject.SetActive(false);
        DoorOn = false;
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
    }

    public void turnOffWindow()
    {
        window.gameObject.SetActive(false);
        WindowOn = false;
    }
}
