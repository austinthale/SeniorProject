using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssailantEditorManager : EditorManager {

    public List<GameObject> assailantList = new List<GameObject>();
    public GameObject assailantPrefab;
    public GameObject GeneralEditorManager;
    EditModePanelEditor editModePanel;
    int _idxSelectedAssailant;

    public override void Add()
    {
        // instantiate, set position to 0,0,0
        GameObject assailant = Instantiate(assailantPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        // add to list
        assailantList.Add(assailant);
        // similar to CameraPlacementEditor
        if (editModePanel.getDropdownVal() == 2)
            editModePanel.changePanelView(2);
    }

    public override void Delete(int ind)
    {
        if (assailantList.Count > 0)
        {
            GameObject temp;
            // assailant has not been selected
            if (ind == -1)
            {
                temp = assailantList[assailantList.Count - 1];
                assailantList.RemoveAt(assailantList.Count - 1);
            }
            else
            {
                temp = assailantList[ind];
                assailantList.RemoveAt(ind);
            }
            Destroy(temp);

            if (editModePanel.getDropdownVal() == 2)
                editModePanel.changePanelView(2);
        }
    }

    public override void Move()
    {
        throw new System.NotImplementedException();
    }

    // Use this for initialization
    void Start()
    {
        editModePanel = GeneralEditorManager.GetComponent<GeneralEditorManager>().canvas.transform.Find("Edit Mode Panel").transform.GetComponent<EditModePanelEditor>();
        _idxSelectedAssailant = -1;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public int getIdxSelectedAssailant()
    {
        return _idxSelectedAssailant;
    }
}
