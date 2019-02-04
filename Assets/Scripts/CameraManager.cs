using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {

    public GameObject GridManage;
    public List<Camera> cameraList;
    int index = 0;


    // Use this for initialization
    void Start () {
		//foreach()
	}
	
	// Update is called once per frame
	void Update () {
		
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
