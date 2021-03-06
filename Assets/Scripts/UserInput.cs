﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class UserInput : MonoBehaviour {

    //public
    public GameObject MainCam;
    public float camZoomSpeed = 1;
    public float camDragSpeed = 1;
    public GameObject gridmanager;
    public GameObject assailant;
    public GameObject secCamCanvas;
    public GameObject zoneCanvas;
    public GameObject editCanvas;
    private List<GameObject> wallList;
    public GameObject GeneralEditorManager;
    private GameObject wallmManager;

    //private
    private Vector3 camDragOrigin;
    private bool isPanning;
    private PlayerController player_controller;
    private float assailant_speed = 1;
    private SelectZone selectZoneScript;

    // Use this for initialization
    void Start () {
        wallmManager = GeneralEditorManager.GetComponent<GeneralEditorManager>().wallEditor;
        gridmanager = GeneralEditorManager.GetComponent<GeneralEditorManager>().gridManager;
        if (assailant != null)
        {
            player_controller = assailant.GetComponent<PlayerController>();
            assailant_speed = assailant.GetComponent<NavMeshAgent>().speed;
        }
        selectZoneScript = this.gameObject.GetComponent<SelectZone>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            {
                MainCam.transform.Translate(0, 0, camZoomSpeed * Time.deltaTime);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                MainCam.transform.Translate(0, 0, -camZoomSpeed * Time.deltaTime);

            }
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
            gridmanager.GetComponent<GenerateGrid>().zoneList.gameObject.SetActive(true);
            //make zones selectable
            foreach (var z in gridmanager.GetComponent<GenerateGrid>().gridList)
            {
                z.GetComponent<ToggleSelection>().selectable = true;
            }
            //List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;
        }
        else if (option == 2) //Camera Mode
        {
            ViewReset(true);
            secCamCanvas.SetActive(true);

            if (assailant != null)
            {
                player_controller.IsSimMode = true;
                assailant.GetComponent<NavMeshAgent>().isStopped = false;
                assailant.GetComponent<NavMeshAgent>().speed = assailant_speed;
            }
        }
        else if (option == 3) //Sim Mode, no zones
        {
            ViewReset(true);
            //player_controller.stopMoving();
            if (assailant != null)
            {
                player_controller.IsSimMode = true;
                assailant.GetComponent<NavMeshAgent>().isStopped = false;
                assailant.GetComponent<NavMeshAgent>().speed = assailant_speed;
            }
        }
        else if (option == 4) //Edit Mode
        {
            ViewReset(false);
            editCanvas.SetActive(true);
            gridmanager.GetComponent<GenerateGrid>().WallPlacementParentObject.gameObject.SetActive(true);
            foreach (var w in gridmanager.GetComponent<GenerateGrid>().WallPlacementList)
            {
                w.gameObject.SetActive(false);
            }
            wallmManager.GetComponent<WallEditorManager>().wallPlacementActive = false;
            //make zones visable but not selectable
            gridmanager.GetComponent<GenerateGrid>().zoneList.gameObject.SetActive(true);
            foreach (var z in gridmanager.GetComponent<GenerateGrid>().gridList)
            {
                z.GetComponent<ToggleSelection>().selectable = false;
            }
        }
    }

    public void ViewReset(bool allowMove)
    {
        if (selectZoneScript.currentZone != null)
            selectZoneScript.currentZone.GetComponent<ToggleSelection>().toggleZone();
        List<GameObject> zoneList = gridmanager.GetComponent<GenerateGrid>().gridList;

        gridmanager.GetComponent<GenerateGrid>().WallPlacementParentObject.gameObject.SetActive(false);
        
        gridmanager.GetComponent<GenerateGrid>().zoneList.gameObject.SetActive(false);
        secCamCanvas.SetActive(false);
        editCanvas.SetActive(false);
        zoneCanvas.SetActive(false);
        if (!allowMove && assailant != null) {
            player_controller.IsSimMode = false;
            assailant.GetComponent<NavMeshAgent>().speed = 0;
            assailant.GetComponent<NavMeshAgent>().isStopped = true;
        }
        
    }
}
