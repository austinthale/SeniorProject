using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FuzzyTools;

public class EditModePanelEditor : MonoBehaviour {

    public Dropdown _dropdown;
    public GameObject _dropdown2;
    public GameObject _addButton;
    public GameObject _removeButton;
    public GameObject _listPanel;
    public GameObject _scrollBar;
    public GameObject _grid;
    public GameObject objButtonPrefab;
    public GameObject General_Manager;
    private WallEditorManager wallManager;
    private CameraManager camManager;
    private GuardEditorManager guardManager;
    private FloorEditorManager floorManager;
    private AssailantEditorManager assailantManager;
    private PropEditorManager propManager;
    private RectTransform panelOrigin = new RectTransform();
    private RectTransform scrollOrigin = new RectTransform();
    private RectTransform gridOrigin = new RectTransform();
    private RectTransform addOrigin = new RectTransform();
    private RectTransform removeOrigin = new RectTransform();


    // Use this for initialization
    void Start () {
        wallManager = General_Manager.GetComponent<GeneralEditorManager>().wallEditor.GetComponent<WallEditorManager>();
        camManager = General_Manager.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>();
        guardManager = General_Manager.GetComponent<GeneralEditorManager>().guardManager.GetComponent<GuardEditorManager>();
        floorManager = General_Manager.GetComponent<GeneralEditorManager>().floorManager.GetComponent<FloorEditorManager>();
        assailantManager = General_Manager.GetComponent<GeneralEditorManager>().assailantManager.GetComponent<AssailantEditorManager>();
        propManager = General_Manager.GetComponent<GeneralEditorManager>().propManager.GetComponent<PropEditorManager>();

        // Save the original RectTransform for UI elements
        if (_listPanel != null)
            panelOrigin = Instantiate<RectTransform>(_listPanel.GetComponent<RectTransform>());
        if (_scrollBar != null)
            scrollOrigin = Instantiate<RectTransform>(_scrollBar.GetComponent<RectTransform>());
        if (_grid != null)
            gridOrigin = Instantiate<RectTransform>(_grid.GetComponent<RectTransform>());
        if (_addButton != null)
            addOrigin = Instantiate<RectTransform>(_addButton.GetComponent<RectTransform>());
        if (_removeButton != null)
            removeOrigin = Instantiate<RectTransform>(_removeButton.GetComponent<RectTransform>());

        _addButton.GetComponent<Button>().onClick.AddListener(AddButtonClicked);
        _removeButton.GetComponent<Button>().onClick.AddListener(RemoveButtonClicked);

        changePanelView(0);
    }

    public void changePanelView(int dropdownVal)
    {
        resetGridList();
        moveUIStuffUp();
        propManager.StopPropCoroutines();
        if (dropdownVal == 0) //layout
        {
            this._dropdown2.SetActive(false);
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);

            GameObject WallEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button wallBtn = WallEditButton.GetComponent<Button>();
            wallBtn.onClick.AddListener(() => wallManager.DoorOff());
            wallBtn.onClick.AddListener(() => wallManager.WindowOff());
            wallBtn.onClick.AddListener(() => wallManager.WallOn());

            GameObject FloorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button floorBtn = FloorEditButton.GetComponent<Button>();
            floorBtn.onClick.AddListener(() => wallManager.WallOff());
            floorBtn.onClick.AddListener(() => floorManager.displayEditFloor());
            //floorBtn.onClick.AddListener(() => floorManager.clearAll()); //delete everything----warning

            GameObject DoorEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button doorBtn = DoorEditButton.GetComponent<Button>();
            doorBtn.onClick.AddListener(() => wallManager.WindowOff());
            doorBtn.onClick.AddListener(() => wallManager.toggleDoorEditMode());
            doorBtn.onClick.AddListener(() => wallManager.WallOn());

            GameObject WindowEditButton = Instantiate(objButtonPrefab, _grid.transform);
            Button winBtn = WindowEditButton.GetComponent<Button>();
            winBtn.onClick.AddListener(() => wallManager.DoorOff());
            winBtn.onClick.AddListener(() => wallManager.toggleWindowEditMode());
            winBtn.onClick.AddListener(() => wallManager.WallOn());


            WallEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Walls";
            FloorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Floor";
            DoorEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Doors";
            WindowEditButton.transform.Find("Text").transform.GetComponent<Text>().text = "Edit Windows";
        }
        else if (dropdownVal == 1) //cameras
        {
            camManager.camModeOn();
            this._dropdown2.SetActive(false);
            this._addButton.SetActive(false);
            this._removeButton.SetActive(false);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            generateButtons(dropdownVal);
        }
        else if (dropdownVal == 2) //guards
        {
            this._dropdown2.SetActive(false);
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);

            generateButtons(dropdownVal);
        }
        else if (dropdownVal == 3) //assailants
        {
            this._dropdown2.SetActive(false);
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            generateButtons(dropdownVal);
        }
        else if (dropdownVal == 4) //props
        {
            this._dropdown2.SetActive(true);
            this._addButton.SetActive(true);
            this._removeButton.SetActive(true);
            this._listPanel.SetActive(true);
            this._scrollBar.SetActive(true);
            moveUIStuffDown();
            generateButtons(dropdownVal);
        }
    }

    private void moveUIStuffUp()
    {
        Debug.Log("AddOrigin's recttrans: " + addOrigin.offsetMax.ToString());
        _addButton.GetComponent<RectTransform>().offsetMax = addOrigin.offsetMax;
        _addButton.GetComponent<RectTransform>().sizeDelta = addOrigin.sizeDelta;

        _removeButton.GetComponent<RectTransform>().offsetMax = removeOrigin.offsetMax;
        _removeButton.GetComponent<RectTransform>().sizeDelta = removeOrigin.sizeDelta;

        _listPanel.GetComponent<RectTransform>().offsetMax = panelOrigin.offsetMax;
        _scrollBar.GetComponent<RectTransform>().offsetMax = scrollOrigin.offsetMax;
    }
    /*************************************************************
    * Note: Left - rectTransform.offsetMin.x;
    *       Right - rectTransform.offsetMax.x;
    *       Top - rectTransform.offsetMax.y;
    *       Bottom - rectTransform.offsetMin.y;
    *************************************************************/
    private void moveUIStuffDown()
    {
        _addButton.GetComponent<RectTransform>().offsetMax = new Vector2(addOrigin.offsetMax.x, -70);
        _addButton.GetComponent<RectTransform>().sizeDelta = new Vector2(addOrigin.sizeDelta.x, 30); // x = width, y = height

        _removeButton.GetComponent<RectTransform>().offsetMax = new Vector2(removeOrigin.offsetMax.x, -70);
        _removeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(removeOrigin.sizeDelta.x, 30); // x = width, y = height

        _listPanel.GetComponent<RectTransform>().offsetMax = new Vector2(panelOrigin.offsetMax.x, 100 * -1);
        _scrollBar.GetComponent<RectTransform>().offsetMax = new Vector2(scrollOrigin.offsetMax.x, 100 * -1);
    }

    public int getDropdownVal()
    {
        return _dropdown.value;
    }

    public int getDropdown2Val()
    {
        return _dropdown2.GetComponent<Dropdown>().value;
    }

    private void resetGridList()
    {
        wallManager.WallOff();
        camManager.camModeOff();
        floorManager.undisplayEditFloor();
        propManager.undisplayCustomizerPanel();
        if (!_grid)
            return;
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
                Button propBtn = temp.GetComponent<Button>();
                //propBtn.onClick.AddListener(() => wallManager.WallOff());
                count++;
            }
        }
        else if (type == 2) // GUARD BUTTONS
        {
            foreach (GameObject guard in guardManager.guardList)
            {
                temp = Instantiate(objButtonPrefab, _grid.transform);
                temp.transform.Find("Text").transform.GetComponent<Text>().text = "Guard " + count;
                Button propBtn = temp.GetComponent<Button>();
                //propBtn.onClick.AddListener(() => wallManager.WallOff());
                count++;
            }
        }
        else if (type == 3) // ASSAILANT BUTTONS
        {
            foreach (GameObject ass in assailantManager.assailantList)
            {
                temp = Instantiate(objButtonPrefab, _grid.transform);
                temp.transform.Find("Text").transform.GetComponent<Text>().text = "Assailant " + count;
                Button propBtn = temp.GetComponent<Button>();
                //propBtn.onClick.AddListener(() => wallManager.WallOff());
                count++;
            }
        }

        else if (type == 4) // PROP BUTTONS
        {
            int chairCount = 1;
            int deskCount = 1;
            int plantCount = 1;
            int index = 0;
            //int propType = getDropdown2Val() + 1;
            foreach (GameObject prop in propManager.propList)
            {
                if (prop.GetComponent<PropEditor>().type == PropEditor.propType.chair)
                {
                    temp = Instantiate(objButtonPrefab, _grid.transform);

                    if (prop.gameObject.name != "Chair")
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = prop.gameObject.name;
                    else
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = "Chair " + chairCount;
                    Button propBtn = temp.GetComponent<Button>();
                    prop.GetComponent<PropEditor>().index = index;
                    propBtn.onClick.AddListener(() => propManager.displayCustomizerPanel(prop.GetComponent<PropEditor>().index));   // When clicked, it pops open a customizer panel for this obj
                    chairCount++;
                }
                else if (prop.GetComponent<PropEditor>().type == PropEditor.propType.desk)
                {
                    temp = Instantiate(objButtonPrefab, _grid.transform);
                    if (prop.gameObject.name != "Desk")
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = prop.gameObject.name;
                    else
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = "Desk " + deskCount;
                    Button propBtn = temp.GetComponent<Button>();
                    prop.GetComponent<PropEditor>().index = index;
                    propBtn.onClick.AddListener(() => propManager.displayCustomizerPanel(prop.GetComponent<PropEditor>().index));
                    deskCount++;
                }
                else if (prop.GetComponent<PropEditor>().type == PropEditor.propType.plant)
                {
                    temp = Instantiate(objButtonPrefab, _grid.transform);
                    if (prop.gameObject.name != "Plant")
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = prop.gameObject.name;
                    else
                        temp.transform.Find("Text").transform.GetComponent<Text>().text = "Plant " + plantCount;
                    Button propBtn = temp.GetComponent<Button>();
                    prop.GetComponent<PropEditor>().index = index;
                    propBtn.onClick.AddListener(() => propManager.displayCustomizerPanel(prop.GetComponent<PropEditor>().index));
                    plantCount++;
                }
                index++;
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
            assailantManager.Add();
            // add assailant
        }
        else if (dropdownVal == 4)
        {
            StopCoroutine(propManager.zoomIn());
            StopCoroutine(propManager.zoomOut());

            //propManager.undisplayCustomizerPanel();
            int propType = getDropdown2Val() + 1;
            if (propType == 1)
            {
                propManager.AddChair();
            }
            else if(propType == 2)
            {
                propManager.AddDesk();
            }
            else if (propType == 3)
            {
                propManager.AddPlant();
            }
        }
    }


    public void RemoveButtonClicked()
    {
        //propManager.undisplayCustomizerPanel();
        int dropdownVal = getDropdownVal();
        if (dropdownVal == 2)
        {
            // remove guard
            guardManager.Delete(guardManager.getIdxSelectedGuard());
        }
        else if (dropdownVal == 3)
        {
            // remove assailant
            assailantManager.Delete(assailantManager.getIdxSelectedAssailant());
        }
        else if (dropdownVal == 4)
        {
            // remove prop...
            int propType = getDropdown2Val() + 1;
            if (propType == 1)
            {
                propManager.RemoveChair(propManager.getIdxSelectedProp());
            }
            else if (propType == 2)
            {
                propManager.RemoveDesk(propManager.getIdxSelectedProp());
            }
            else if (propType == 3)
            {
                propManager.RemovePlant(propManager.getIdxSelectedProp());
            }
        }
    }
}
