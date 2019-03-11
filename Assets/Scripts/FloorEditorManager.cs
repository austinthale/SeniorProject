using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorEditorManager : MonoBehaviour {
    public GameObject floor;

    public GameObject General;

    public WallEditorManager wallManager;
    public GenerateGrid gridManager;
    public CameraManager camManager;
    public GuardEditorManager guardManager;

    private void Start()
    {
        wallManager = General.GetComponent<GeneralEditorManager>().wallEditor.GetComponent<WallEditorManager>();
        gridManager = General.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>();
        camManager = General.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>();
        guardManager = General.GetComponent<GeneralEditorManager>().guardManager.GetComponent<GuardEditorManager>();
    }

    public void clearAll()
    {
        wallManager.clearAll();
        gridManager.clearAll();
        camManager.clearAll();
        genNewGrid(8);
    }

    public void genNewGrid(int num)
    {
        gridManager.gridHeight = num;
        gridManager.gridWidth = num;
        gridManager.genZones();
    }
}
