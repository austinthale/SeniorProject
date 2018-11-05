using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {

    //public
    public GameObject MainCam;
    public float camZoomSpeed = 1;
    public float camDragSpeed = 1;

    //private
    private Vector3 camDragOrigin;
    private bool isPanning;



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            MainCam.transform.Translate(0, 0, camZoomSpeed * Time.deltaTime);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            MainCam.transform.Translate(0, 0, -camZoomSpeed * Time.deltaTime);
        }

        if (Input.GetMouseButtonDown(1))
        {
            //right click was pressed    
            camDragOrigin = Input.mousePosition;
            isPanning = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - camDragOrigin);

            // update x and y but not z
            Vector3 move = new Vector3(pos.x * camDragSpeed, pos.y * camDragSpeed, 0);

            Camera.main.transform.Translate(-move, Space.Self);
            if (Camera.main.transform.position == camDragOrigin)
            {
                isPanning = false;
            }
        }
        

    }
}
