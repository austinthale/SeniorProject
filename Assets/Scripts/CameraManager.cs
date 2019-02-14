using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {

    public GameObject GridManage;
    public List<Camera> cameraList;
    public List<GameObject> CameraPlacements;
    public bool CamPlacementActive = false;
    int index = 0;


    // Use this for initialization
    void Start () {
		//foreach()
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("o"))
        {
            toggleCamEditMode();
        }
    }

    public void toggleCamEditMode()
    {
        if (CamPlacementActive)
        {
            foreach (var cam in CameraPlacements)
            {
                cam.gameObject.SetActive(false);
            }
            CamPlacementActive = false;
        }
        else
        {
            foreach (var cam in CameraPlacements)
            {
                cam.gameObject.SetActive(true);
            }
            CamPlacementActive = true;
        }
    }

    public void getCurrentCamPlacements()
    {
        foreach (var wall in GridManage.GetComponent<GenerateGrid>().walls)
        {
            if (wall.gameObject.activeSelf)
            {

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
