using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditModePanelEditor : MonoBehaviour {

    private Dropdown _dropdown;
    private GameObject _addButton;
    private GameObject _removeButton;
    private GameObject _listPanel;
    private GameObject _scrollBar;
    private GameObject _grid;
    public GameObject objButtonPrefab;
    public GameObject General_Manager;
    private WallEditorManager wallManager;
    private CameraManager camManager;
    private GuardEditorManager guardManager;


    // Use this for initialization
    void Start () {
        wallManager = General_Manager.GetComponent<GeneralEditorManager>().wallEditor.GetComponent<WallEditorManager>();
        camManager = General_Manager.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>();
        guardManager = General_Manager.GetComponent<GeneralEditorManager>().guardManager.GetComponent<GuardEditorManager>();
        this._dropdown = this.transform.Find("Dropdown").GetComponent<Dropdown>();
        this._addButton = this.transform.Find("AddButton").gameObject;
        _addButton.GetComponent<Button>().onClick.AddListener(AddButtonClicked);
        this._removeButton = this.transform.Find("RemoveButton").gameObject;
        _removeButton.GetComponent<Button>().onClick.AddListener(RemoveButtonClicked);
        this._listPanel = this.transform.Find("ListPanel").gameObject;
        this._scrollBar = this.transform.Find("Scrollbar").gameObject;
        this._grid = _listPanel.transform.Find("Grid").gameObject;
        changePanelView(0);
    }


    public void changePanelView(int dropdownVal)
    {
        if (dropdownVal == 0) //layout
        {
            resetGridList();
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            GameObject WallEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button wallBtn = WallEditButton.GetComponent<Button>();
            wallBtn.onClick.AddListener(() => wallManager.toggleWallEditMode());
            GameObject FloorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button floorBtn = FloorEditButton.GetComponent<Button>();
            floorBtn.onClick.AddListener(() => wallManager.WallOff());
            GameObject DoorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button doorBtn = DoorEditButton.GetComponent<Button>();
            doorBtn.onClick.AddListener(() => wallManager.WallOff());
            GameObject WindowEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button winBtn = WindowEditButton.GetComponent<Button>();
            winBtn.onClick.AddListener(() => wallManager.WallOff());
            WallEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Walls";
            FloorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Floor";
            DoorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Doors";
            WindowEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Windows";
        }
        else if (dropdownVal == 1) //cameras
        {
            resetGridList();
            camManager.camModeOn();
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            generateButtons(dropdownVal);
        }
        else if (dropdownVal == 2) //guards
        {
            resetGridList();
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);

            generateButtons(dropdownVal);
        }
        else if (dropdownVal == 3) //assailants
        {
            resetGridList();
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
        }
        else if (dropdownVal == 4) //props
        {
            resetGridList();
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(false);
            this._scrollBar.SetActive(false);
        }
    }


    public int getDropdownVal()
    {
        return _dropdown.value;
    }

    private void resetGridList()
    {
        wallManager.WallOff();
        camManager.camModeOff();
        foreach (Transform t in _grid.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    public void generateButtons(int type)
    {
        GameObject temp;
        int count = 1;
        if (type == 1) // CAMERA BUTTONS
        {
            foreach (Camera cam in camManager.cameraList)
            {
                temp = Instantiate(objButtonPrefab, _grid.transform);
                temp.transform.Find("Text").transform.GetComponent<Text>().text = "Camera " + count;
                count++;
            }
        }
        else if (type == 2) // GUARD BUTTONS
        {
            foreach (GameObject guard in guardManager.guardList)
            {
                temp = Instantiate(objButtonPrefab, _grid.transform);
                temp.transform.Find("Text").transform.GetComponent<Text>().text = "Guard " + count;
                count++;
            }
        }
    }


    public void AddButtonClicked()
    {
        int dropdownVal = getDropdownVal();
        if (dropdownVal == 2)
        {
            // add guard
            guardManager.Add();
        }
        else if (dropdownVal == 3)
        {
            // add assailant
        }
        else if (dropdownVal == 4)
        {
            // add others...
        }
    }


    public void RemoveButtonClicked()
    {
        int dropdownVal = getDropdownVal();
        if (dropdownVal == 2)
        {
            // add guard
            guardManager.Delete(guardManager.getIdxSelectedGuard());
        }
        else if (dropdownVal == 3)
        {
            // add assailant
        }
        else if (dropdownVal == 4)
        {
            // add others...
        }
    }


}
