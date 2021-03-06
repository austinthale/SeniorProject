﻿using System.Collections;
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

    // Use this for initialization
    void Start () {
        this._dropdown = this.transform.Find("Dropdown").GetComponent<Dropdown>();
        this._addButton = this.transform.Find("AddButton").gameObject;
        this._removeButton = this.transform.Find("RemoveButton").gameObject;
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
            GameObject FloorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            GameObject DoorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            GameObject WindowEditButton = Instantiate(objButtonPrefab, _grid.transform);
            WallEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Walls";
            FloorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Floor";
            DoorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Doors";
            WindowEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Windows";
        }
        else if (dropdownVal == 1) //cameras
        {
            resetGridList();
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            generateCameraButtons();
        }
        else if (dropdownVal == 2) //guards
        {
            resetGridList();
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
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
        foreach (Transform t in _grid.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    public void generateCameraButtons()
    {
        GameObject temp;
        int count = 1;
        foreach (Camera cam in GameObject/*.Find("EditorManager").transform*/.Find("CameraManager").transform.GetComponent<CameraManager>().cameraList)
        {
            temp = Instantiate(objButtonPrefab, _grid.transform);
            temp.transform.Find("Text").transform.GetComponent<Text>().text = "Camera " + count;
            count++;
        }
    }
}
