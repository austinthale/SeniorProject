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
    private GameObject canvas;
    EditModePanelEditor editModePanel;

    private GameObject cameraParent;

    public void Start()
    {
       cameraParent = GeneralEditorManager.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>().CameraParent;
       CameraEditor = GeneralEditorManager.GetComponent<GeneralEditorManager>().cameraManager;
       canvas = GeneralEditorManager.GetComponent<GeneralEditorManager>().canvas;
       editModePanel = canvas.transform.Find("Edit Mode Panel").transform.GetComponent<EditModePanelEditor>();
    }

    private void OnMouseDown()
    {
        toggleCam();
    }
    public void toggleCam()
    {
        if (!on)
        {
            camOn();
        }
        else
        {
            camOff();
        }
    }

    public void camOff()
    {
        camera.gameObject.transform.parent = this.transform;
        CameraEditor.GetComponent<CameraManager>().cameraList.Remove(actaulCamera.GetComponent<Camera>());
        if (editModePanel.getDropdownVal() == 1)
            editModePanel.changePanelView(1);
        camera.gameObject.SetActive(false);
        on = false;
    }
    public void camOn()
    {
        camera.gameObject.SetActive(true);
        CameraEditor.GetComponent<CameraManager>().cameraList.Add(actaulCamera.GetComponent<Camera>());
        camera.gameObject.transform.parent = cameraParent.transform;
        if (editModePanel.getDropdownVal() == 1)
            editModePanel.changePanelView(1);
        on = true;
    }
}
