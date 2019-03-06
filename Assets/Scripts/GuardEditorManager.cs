using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardEditorManager : EditorManager {

    public List<GameObject> guardList = new List<GameObject>();
    public GameObject guardPrefab;
    public GameObject GeneralEditorManager;
    EditModePanelEditor editModePanel;
    int _idxSelectedGuard;

    public override void Add()
    {
        // instantiate, set position to 0,0,0
        GameObject guard = Instantiate(guardPrefab, new Vector3(0,0,0), new Quaternion(0,0,0,0));
        // add to list
        guardList.Add(guard);
        // similar to CameraPlacementEditor
        if (editModePanel.getDropdownVal() == 2)
            editModePanel.changePanelView(2);
    }

    public override void Delete(int ind)
    {
        if (guardList.Count > 0)
        {
            GameObject temp;
            // guard has not been selected
            if (ind == -1)
            {
                temp = guardList[guardList.Count - 1];
                guardList.RemoveAt(guardList.Count - 1);
            }
            else
            {
                temp = guardList[ind];
                guardList.RemoveAt(ind);
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
    void Start () {
        editModePanel = GeneralEditorManager.GetComponent<GeneralEditorManager>().canvas.transform.Find("Edit Mode Panel").transform.GetComponent<EditModePanelEditor>();
        _idxSelectedGuard = -1;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public int getIdxSelectedGuard()
    {
        return _idxSelectedGuard;
    }
}
