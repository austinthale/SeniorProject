using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlacementEditor : MonoBehaviour {

    public GameObject camera;
    public GameObject actaulCamera;
    public GameObject wall;
    public GameObject GeneralEditorManager;
    public bool hasCamera = false;
    public bool on = false;
    private GameObject CameraEditor;

    private GameObject cameraParent;

    public void Start()
    {
       cameraParent = GeneralEditorManager.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>().CameraParent;
       CameraEditor = GeneralEditorManager.GetComponent<GeneralEditorManager>().cameraManager;
    }

    private void OnMouseDown()
    {
        toggleCam();
    }
    public void toggleCam()
    {
        if (!on)
        {
            camera.gameObject.SetActive(true);
            CameraEditor.GetComponent<CameraManager>().cameraList.Add(actaulCamera.GetComponent<Camera>());
            camera.gameObject.transform.parent = cameraParent.transform;
            on = true;
        }
        else
        {
            camera.gameObject.transform.parent = this.transform;
            camera.gameObject.SetActive(false);
            on = false;
        }
    }
}
