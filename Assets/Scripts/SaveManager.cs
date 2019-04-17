using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour {

    public GameObject GeneralEditor;
    public string SaveName = "SaveData";

    public void save()
    {
        SaveModel CurrentStatus = getSaveModel();

        string json = JsonUtility.ToJson(CurrentStatus);
        print(Application.dataPath);

        System.IO.File.WriteAllText(Application.dataPath + "/" + SaveName, json);

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

    public void LoadSave(string save)
    {
        SaveModel saveData = parse(save);
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

    public SaveModel parse(string file)
    {
        string json = System.IO.File.ReadAllText(Application.dataPath + "/" + SaveName); //for testing purposes...
        //string json = System.IO.File.ReadAllText(file); //when able to select file gets figured out...
        SaveModel save = JsonUtility.FromJson<SaveModel>(json);
        return save;
    }



}
