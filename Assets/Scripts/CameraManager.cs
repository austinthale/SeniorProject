using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {

    public GameObject GridManage;
    public List<Camera> cameraList;
    public List<GameObject> CameraPlacements;
    public GameObject CameraParent;
    public bool CamPlacementActive = false;
    int index;

    // Use this for initialization
    void Start () {
        //foreach()
        index = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("o"))
        {
            toggleCamEditMode();
        }
    }

    public void clearAll()
    {
        foreach (Transform cam in CameraParent.transform)
        {
            Object.Destroy(cam.gameObject);
        }
        cameraList.Clear();
        CameraPlacements.Clear();
    }

    public void toggleCamEditMode()
    {
        if (CamPlacementActive)
        {
            camModeOff();
        }
        else
        {
            camModeOn();
        }
    }

    public void camModeOff()
    {
        foreach (var cam in CameraPlacements)
        {
            cam.gameObject.SetActive(false);
        }
        CamPlacementActive = false;
        emptyCamPlacementList();
    }

    public void camModeOn()
    {
        getCurrentCamPlacements();
        foreach (var cam in CameraPlacements)
        {
            cam.gameObject.SetActive(true);
        }
        CamPlacementActive = true;
    }

    public void emptyCamPlacementList()
    {
        CameraPlacements.Clear();
    }

    public void getCurrentCamPlacements()
    {
        foreach (var wall in GridManage.GetComponent<GenerateGrid>().walls)
        {
            if (wall.gameObject.activeSelf)
            {
                CameraPlacements.Add(wall.GetComponent<WallEditor>().CameraPlacements[0]);
                CameraPlacements.Add(wall.GetComponent<WallEditor>().CameraPlacements[1]);
            }
        }
    }

    public void nextClicked()
    {
        cameraList[index].gameObject.SetActive(false);
        if (index < cameraList.Count - 1)
        {
            index++;
        }
        else
        {
            index = 0;
        }
        cameraList[index].gameObject.SetActive(true);
    }

    public void prevClicked()
    {
        cameraList[index].gameObject.SetActive(false);
        if (index > 0)
        {
            index--;
        }
        else
        {
            index = cameraList.Count - 1;
        }
        cameraList[index].gameObject.SetActive(true);
    }
}
