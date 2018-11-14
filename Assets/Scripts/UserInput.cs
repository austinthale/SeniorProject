using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UserInput : MonoBehaviour {

    //public
    public GameObject MainCam;
    public float camZoomSpeed = 1;
    public float camDragSpeed = 1;
    public GameObject gridmanager;
    public GameObject assailant;

    //private
    private Vector3 camDragOrigin;
    private bool isPanning;
    private PlayerController player_controller;
    private float assailant_speed = 1;


    // Use this for initialization
    void Start () {
        player_controller = assailant.GetComponent<PlayerController>();
        assailant_speed = assailant.GetComponent<NavMeshAgent>().speed;

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

    public void ToggleView(float option)
    {
        if (option == 1) //Zone Mode
        {
            ViewReset();
            gridmanager.SetActive(true);
            player_controller.IsSimMode = false;
            List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;
            //assailant.transform.position = assailant.GetComponent<NavMeshAgent>().destination;
            assailant.GetComponent<NavMeshAgent>().speed = 0;
            assailant.GetComponent<NavMeshAgent>().isStopped = true;
        }
        else if (option == 2) //See through Zones
        {
            ViewReset();
        }
        else if (option == 3) //No Zone
        {
            ViewReset();
            //player_controller.stopMoving();
            player_controller.IsSimMode = true;
            assailant.GetComponent<NavMeshAgent>().isStopped = false;
            assailant.GetComponent<NavMeshAgent>().speed = assailant_speed;
        }
    }

    public void ViewReset()
    {
        List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;
        player_controller.IsSimMode = false;
        gridmanager.SetActive(false);
    }
}
