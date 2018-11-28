using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {

    public GameObject GridManage;
    public GameObject CameraPanel;
    public GameObject Camera_Image;
    public List<Camera> cameraList;
    int index = 0;


    // Use this for initialization
    void Start () {
		//foreach()
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void nextClicked()
    {
        if (index < cameraList.Count)
        {
            //Camera_Image.
            index++;
        }
        else
        {
            index = 0;
        }
    }
}
