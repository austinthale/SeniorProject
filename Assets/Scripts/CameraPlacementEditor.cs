using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlacementEditor : MonoBehaviour {

    public GameObject camera;
    public GameObject wall;
    public GameObject GeneralEditorManager;
    public bool hasCamera = false;
    public bool on = false;

    private void OnMouseDown()
    {
        toggleCam();
    }
    public void toggleCam()
    {
        if (!on)
        {
            camera.gameObject.SetActive(true);
            on = true;
        }
        else
        {
            camera.gameObject.SetActive(false);
            on = false;
        }
    }
}
