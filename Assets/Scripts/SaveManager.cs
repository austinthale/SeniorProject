using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;

public class SaveManager : MonoBehaviour {

    public GameObject GeneralEditor;
    public string SaveName = "SaveData";
    public string path = "";
    private string json = "";

    public void save()
    {
        SaveModel CurrentStatus = getSaveModel();

        json = JsonUtility.ToJson(CurrentStatus);
        //print(Application.dataPath);
        path = StandaloneFileBrowser.SaveFilePanel("Title", "", "SaveData", "txt").ToString();
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, json);
        }
        //readableReport();

        Debug.Log("Report Genereated");
    }



    public SaveModel getSaveModel()
    {
        SaveModel Save = new SaveModel();
        
        foreach (GameObject placement in GeneralEditor.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>().WallPlacementList)
        {

            Save.WallTypes.Add(placement.GetComponent<WallPlacementEditor>().currentType);
        }
        Save.gridSzie = GeneralEditor.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>().gridHeight;
        return Save;
    }

    public void LoadSave()
    {


        SaveModel saveData = parse();
        GeneralEditor.GetComponent<GeneralEditorManager>().floorManager.GetComponent<FloorEditorManager>().clearAll();
        GeneralEditor.GetComponent<GeneralEditorManager>().floorManager.GetComponent<FloorEditorManager>().LoadGrid(saveData.gridSzie);
        for (int i = 0; i < saveData.WallTypes.Count; i++)
        {
            if (saveData.WallTypes[i] == WallPlacementEditor.WallType.Wall)
            {
                GeneralEditor.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>().WallPlacementList[i].GetComponent<WallPlacementEditor>().toggleWall();
            }
            else if (saveData.WallTypes[i] == WallPlacementEditor.WallType.Door)
            {
                GeneralEditor.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>().WallPlacementList[i].GetComponent<WallPlacementEditor>().toggleDoor();
            }
            else if (saveData.WallTypes[i] == WallPlacementEditor.WallType.Window)
            {
                GeneralEditor.GetComponent<GeneralEditorManager>().gridManager.GetComponent<GenerateGrid>().WallPlacementList[i].GetComponent<WallPlacementEditor>().toggleWindow();
            }
        }
    }

    public SaveModel parse()
    {
        var temp = StandaloneFileBrowser.OpenFilePanel("Title", "", "txt", false);
        path = temp[0];

        json = System.IO.File.ReadAllText(path); //for testing purposes...
        //string json = System.IO.File.ReadAllText(file); //when able to select file gets figured out...
        SaveModel save = JsonUtility.FromJson<SaveModel>(json);
        return save;
    }

}
