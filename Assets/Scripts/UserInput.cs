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
    public GameObject secCamCanvas;
    public GameObject zoneCanvas;

    //private
    private Vector3 camDragOrigin;
    private bool isPanning;
    private PlayerController player_controller;
    private float assailant_speed = 1;
    private SelectZone selectZoneScript;

    // Use this for initialization
    void Start () {
        player_controller = assailant.GetComponent<PlayerController>();
        assailant_speed = assailant.GetComponent<NavMeshAgent>().speed;
        selectZoneScript = this.gameObject.GetComponent<SelectZone>();
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
            ViewReset(false);
            gridmanager.SetActive(true);
            //List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;
        }
        else if (option == 2) //Camera Mode
        {
            ViewReset(true);
            secCamCanvas.SetActive(true);
            player_controller.IsSimMode = true;
            assailant.GetComponent<NavMeshAgent>().isStopped = false;
            assailant.GetComponent<NavMeshAgent>().speed = assailant_speed;
        }
        else if (option == 3) //Sim Mode, no zones
        {
            ViewReset(true);
            //player_controller.stopMoving();
            player_controller.IsSimMode = true;
            assailant.GetComponent<NavMeshAgent>().isStopped = false;
            assailant.GetComponent<NavMeshAgent>().speed = assailant_speed;
        }
    }

    public void ViewReset(bool allowMove)
    {
        if (selectZoneScript.currentZone != null)
            selectZoneScript.currentZone.GetComponent<ToggleSelection>().toggleZone();
        List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;

        gridmanager.SetActive(false);
        secCamCanvas.SetActive(false);
        zoneCanvas.SetActive(false);
        if (!allowMove) {
            player_controller.IsSimMode = false;
            assailant.GetComponent<NavMeshAgent>().speed = 0;
            assailant.GetComponent<NavMeshAgent>().isStopped = true;
        }
        
    }
}
