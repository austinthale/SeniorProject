using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPlacementEditor : MonoBehaviour
{

    public bool directionX = false;
    public bool directionZ = false;
    public bool hasWall = false;
    public GameObject wall;
    public bool on = false;

    private void OnMouseDown()
    {
        toggleWall();
    }
    public void toggleWall()
    {
        if (!on)
        {
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
}
