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


    // Use this for initialization
    void Start () {
        this._dropdown = this.transform.Find("Dropdown").GetComponent<Dropdown>();
        this._addButton = this.transform.Find("AddButton").gameObject;
        this._removeButton = this.transform.Find("RemoveButton").gameObject;
        this._listPanel = this.transform.Find("ListPanel").gameObject;
        this._scrollBar = this.transform.Find("Scrollbar").gameObject;
        changePanelView(0);
    }
	
	public void changePanelView(int dropdownVal)
    {
        if (dropdownVal == 0) //layout
        {
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(false);
            this._scrollBar.SetActive(false);
        }
        else if (dropdownVal == 1) //cameras
        {
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(false);
            this._scrollBar.SetActive(false);
        }
        else if (dropdownVal == 2) //guards
        {
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
        }
        else if (dropdownVal == 3) //assailants
        {
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
        }
        else if (dropdownVal == 4) //props
        {
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
}
