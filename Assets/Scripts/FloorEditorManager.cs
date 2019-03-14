using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorEditorManager : MonoBehaviour
{
    public GameObject floor;
    public GameObject floorPanel;
    public GameObject General;
    private Slider slider;
    public WallEditorManager wallManager;
    public GenerateGrid gridManager;
    public CameraManager camManager;
    public GuardEditorManager guardManager;
    public Text sliderSize;
    public int floorSize;

    private void Start()
    {
        floorPanel = General.GetComponent<GeneralEditorManager>().canvas.transform.Find("Floor Edit Panel").gameObject;
        slider = floorPanel.transform.Find("Slider").GetComponent<Slider>();
        wallManager = General.GetComponent<GeneralEditorManager>().wallEditor.GetComponent<WallEditorManager>();
        gridManager = General.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>();
        camManager = General.GetComponent<GeneralEditorManager>().cameraManager.GetComponent<CameraManager>();
        guardManager = General.GetComponent<GeneralEditorManager>().guardManager.GetComponent<GuardEditorManager>();
        floorSize = 2;

    }

    public void clearAll()
    {
        wallManager.clearAll();
        gridManager.clearAll();
        camManager.clearAll();
        //genNewGrid();
    }

    public void genNewGrid()
    {
        gridManager.gridHeight = floorSize;
        gridManager.gridWidth = floorSize;
        gridManager.genZones();
    }

    public void updateFloorSize()
    {
        floorSize = (int)slider.value * 2;
        sliderSize.text = floorSize.ToString() + " x " + floorSize.ToString();
    }

    public void displayEditFloor()
    {
        floorPanel.gameObject.SetActive(true);
    }

    public void undisplayEditFloor()
    {
        floorPanel.gameObject.SetActive(false);
    }

    public void okClicked()
    {
        clearAll();
        genNewGrid();
        undisplayEditFloor();
    }
    public void cancelClicked()
    {
        undisplayEditFloor();
    }
}
