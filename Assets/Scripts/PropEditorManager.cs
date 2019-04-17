using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropEditorManager : MonoBehaviour {

    public List<GameObject> propList;
    public GameObject GeneralEditorManager;
    EditModePanelEditor editModePanelEditor;
    public GameObject objectEditorPanel;
    public GameObject chairPrefab;
    public GameObject deskPrefab;
    public GameObject plantPrefab;
    private GameObject InputField;
    public GameObject PropHolder;

    public Camera mainCamera;
    private Vector3 camOrigPos;
    private Quaternion camOrigRot;
    private float zoomSpeed = 40.0f;
    private float lerpSpeed = 0.1f;
    bool inPosition = false;
    //bool inRotation = false;

    int _idxSelectedProp;

    void Start()
    {
        editModePanelEditor = GeneralEditorManager.GetComponent<GeneralEditorManager>().canvas.transform.Find("Edit Mode Panel").transform.GetComponent<EditModePanelEditor>();
        objectEditorPanel = GeneralEditorManager.GetComponent<GeneralEditorManager>().canvas.transform.Find("Object Editor Panel").gameObject;
        InputField = objectEditorPanel.transform.Find("InputField").gameObject;
        if (mainCamera != null)
        {
            camOrigPos = mainCamera.transform.position;
            camOrigRot = mainCamera.transform.rotation;
        }
        _idxSelectedProp = -1;
    }

    public void AddChair()
    {
        // instantiate, set position to 0,0,0
        GameObject chair = Instantiate(chairPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        chair.name = "Chair";
        chair.transform.parent = PropHolder.transform;
        // add to list
        propList.Add(chair);
        // refresh the buttons in edit mode panel
        if (editModePanelEditor.getDropdownVal() == 4)
            editModePanelEditor.changePanelView(4);
    }
    public void AddDesk()
    {
        // instantiate, set position to 0,0,0
        GameObject desk = Instantiate(deskPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        desk.name = "Desk";
        desk.transform.parent = PropHolder.transform;
        // add to list
        propList.Add(desk);
        // refresh the buttons in edit mode panel
        if (editModePanelEditor.getDropdownVal() == 4)
            editModePanelEditor.changePanelView(4);
    }
    public void AddPlant()
    {
        // instantiate, set position to 0,0,0
        GameObject plant = Instantiate(plantPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        plant.name = "Plant";
        plant.transform.parent = PropHolder.transform;
        // add to list
        propList.Add(plant);
        // refresh the buttons in edit mode panel
        if (editModePanelEditor.getDropdownVal() == 4)
            editModePanelEditor.changePanelView(4);
    }

    public void RemoveChair(int ind)
    {
        if (propList.Count > 0)
        {
            int tempIdx = propList.Count - 1;
            GameObject temp;
            // guard has not been selected
            if (ind == -1)
            {
                temp = propList[tempIdx];
                while (tempIdx >= 0 && temp.GetComponent<PropEditor>().type != PropEditor.propType.chair)
                    tempIdx--;
                if (tempIdx != -1)
                    propList.RemoveAt(tempIdx);
            }
            else
            {
                temp = propList[ind];
                if (temp.GetComponent<PropEditor>().type == PropEditor.propType.chair)
                    propList.RemoveAt(ind);
            }
            if (temp.GetComponent<PropEditor>().type == PropEditor.propType.chair)
                Destroy(temp);

            if (editModePanelEditor.getDropdownVal() == 4)
                editModePanelEditor.changePanelView(4);
        }
    }

    public int getIdxSelectedProp()
    {
        return _idxSelectedProp;
    }

    public void RemoveDesk(int ind)
    {
        if (propList.Count > 0)
        {
            int tempIdx = propList.Count - 1;
            GameObject temp;
            // guard has not been selected
            if (ind == -1)
            {
                temp = propList[tempIdx];
                while (tempIdx >= 0 && temp.GetComponent<PropEditor>().type != PropEditor.propType.desk)
                    tempIdx--;
                if (tempIdx != -1)
                    propList.RemoveAt(tempIdx);
            }
            else
            {
                temp = propList[ind];
                if (temp.GetComponent<PropEditor>().type == PropEditor.propType.desk)
                    propList.RemoveAt(ind);
            }
            if (temp.GetComponent<PropEditor>().type == PropEditor.propType.desk)
                Destroy(temp);

            if (editModePanelEditor.getDropdownVal() == 4)
                editModePanelEditor.changePanelView(4);
        }
    }

    public void RemovePlant(int ind)
    {
        if (propList.Count > 0)
        {
            int tempIdx = propList.Count - 1;
            GameObject temp;
            // guard has not been selected
            if (ind == -1)
            {
                temp = propList[tempIdx];
                while (tempIdx >= 0 && temp.GetComponent<PropEditor>().type != PropEditor.propType.plant)
                    tempIdx--;
                if (tempIdx != -1)
                    propList.RemoveAt(tempIdx);
            }
            else
            {
                temp = propList[ind];
                if (temp.GetComponent<PropEditor>().type == PropEditor.propType.plant)
                    propList.RemoveAt(ind);
            }
            if (temp.GetComponent<PropEditor>().type == PropEditor.propType.plant)
                Destroy(temp);

            if (editModePanelEditor.getDropdownVal() == 4)
                editModePanelEditor.changePanelView(4);
        }
    }

    public void displayCustomizerPanel(int index)
    {
        if (objectEditorPanel.gameObject.activeInHierarchy)
        {
            undisplayCustomizerPanel();
            StopCoroutine(zoomOut());
        }
        //Debug.Log(index);
        _idxSelectedProp = index;
        objectEditorPanel.gameObject.SetActive(true);
        InputField.transform.Find("Text").gameObject.GetComponent<Text>().text = "";
        GameObject placeholderText = InputField.transform.Find("Placeholder").gameObject;
        placeholderText.GetComponent<Text>().text = propList[index].gameObject.name;
        StopCoroutine(zoomIn());
        StartCoroutine(zoomIn());
    }

    public void saveName()
    {
        string text = InputField.transform.Find("Text").gameObject.GetComponent<Text>().text;
        if (text != "")
            propList[_idxSelectedProp].name = text;
        //Call Generate Buttons for updated name
        editModePanelEditor.changePanelView(4);
    }

    public void rotateLeft()
    {
        propList[_idxSelectedProp].transform.Rotate(0, -90, 0);
    }

    public void rotateRight()
    {
        propList[_idxSelectedProp].transform.Rotate(0, 90, 0);
    }

    public void undisplayCustomizerPanel()
    {
        objectEditorPanel.gameObject.SetActive(false);
        StopCoroutine(zoomOut());
        StartCoroutine(zoomOut());
    }

    public IEnumerator zoomIn()
    {
        inPosition = false;
        // Save camera's original position
        camOrigPos = mainCamera.transform.position;
        camOrigRot = mainCamera.transform.rotation;

        Debug.Log(camOrigPos.ToString());

        while (!inPosition/* && !inRotation*/)
        {
            Debug.Log("In ZoomIn");
            Vector3 targetPosition = propList[_idxSelectedProp].transform.position;
            //Quaternion targetRotation = propList[_idxSelectedProp].transform.rotation;

            targetPosition.z += 6;
            targetPosition.x += 13;
            targetPosition.y += 25;
            //targetRotation.x -= 1;

            if (!inPosition)
                mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, targetPosition, zoomSpeed * Time.deltaTime);
            //if (!inRotation)
            //    mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);

            //check distance, and stop when in position
            if(Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.1)
            {
                mainCamera.transform.position = targetPosition;
                inPosition = true;
            }

            //if (Mathf.Abs(mainCamera.transform.localEulerAngles.y - propList[_idxSelectedProp].transform.localEulerAngles.y) < 3)
            //{
            //    inRotation = true;
            //}


            yield return new WaitForEndOfFrame();
        }
        Debug.Log("End zoomin");
    }

    public IEnumerator zoomOut()
    {
        inPosition = false;
        Debug.Log(camOrigPos.ToString());

        Vector3 targetPosition = camOrigPos;


        while (!inPosition/* && !inRotation*/)
        {
            Vector3 fromPosition = mainCamera.transform.position;
            Debug.Log("In ZoomOut");
            //Quaternion targetRotation = propList[_idxSelectedProp].transform.rotation;
            //targetRotation.x -= 1;

            if (!inPosition)
                mainCamera.transform.position = Vector3.MoveTowards(fromPosition, targetPosition, zoomSpeed * Time.deltaTime);
            //if (!inRotation)
            //    mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);

            //check distance, and stop when in position
            if (Mathf.Abs(Vector3.Distance(fromPosition, targetPosition)) < 0.1)
            {
                mainCamera.transform.position = targetPosition;
                inPosition = true;
            }

            //if (Mathf.Abs(mainCamera.transform.localEulerAngles.y - propList[_idxSelectedProp].transform.localEulerAngles.y) < 3)
            //{
            //    inRotation = true;
            //}


            yield return new WaitForEndOfFrame();
        }
    }

    public void StopPropCoroutines()
    {
        StopCoroutine(zoomIn());
        StopCoroutine(zoomOut());

    }

    public void RemoveProp()
    {
        GameObject temp;
        temp = propList[_idxSelectedProp];
        propList.RemoveAt(_idxSelectedProp);
        Destroy(temp);

        if (editModePanelEditor.getDropdownVal() == 4)
            editModePanelEditor.changePanelView(4);
    }

}
