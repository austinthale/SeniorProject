using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;


public class ReportManager : MonoBehaviour {

    public GameObject GeneralManager;

    private GeneralEditorManager General;
    private GenerateGrid GridManager;

    private List<GameObject> zoneList;


    public void Start()
    {
        General = GeneralManager.GetComponent<GeneralEditorManager>();
        GridManager = General.gridManager.GetComponent<GenerateGrid>();
        zoneList = GridManager.gridList;
    }





    /*public GameObject GridManage;
    public string ReportName = "report.txt";
    public report rep = new report();
    public GameObject Report_Panel;
    public Text Report_Panel_Text;
    public Text Report_Panel_Text2;
    public Text Report_Panel_Text3;
    public Text Report_Panel_Text4;
    public bool onOff = true;*/

    /*public report createReport(List<GameObject> zones)
    {
        
        for (int i = 0; i < zones.Count; i++)
        {
            rep.Zones.Add(zones[i]);
            rep.zoneNames.Add(zones[i].name);
            rep.zonePosition.Add(zones[i].transform.position);
            rep.zoneThreatLevel.Add(zones[i].GetComponent<ZoneAnalysis>().threatLevel);
            rep.Number_Guards.Add(zones[i].GetComponent<ZoneAnalysis>().number_guards);
            rep.Assailant_pass_throughs.Add(zones[i].GetComponent<ZoneAnalysis>().times_assialant_passed_through);
            rep.windows.Add(zones[i].GetComponent<ZoneAnalysis>().windows);
            rep.doors.Add(zones[i].GetComponent<ZoneAnalysis>().doors);
        }

        return rep;
    }*/

    /*public void OnEnable()
    {
        onOff = true;
    }*/

    /*private void Update()
    {

    }

    [ExecuteInEditMode]
    public void SaveReport()
    {
        // 1
        report repo = createReport(GridManage.GetComponent<GenerateGrid>().gridList);

        string json = JsonUtility.ToJson(repo);
        print(Application.dataPath);

        System.IO.File.WriteAllText(Application.dataPath + "/" + ReportName, json);

        readableReport();

        Debug.Log("Report Genereated");
    }*/

    /*public void readableReport()
    {
        float amount = rep.zoneNames.Count / 4;
        string temp = "Security Report: " + "\n\n";
        for (int i = 0; i < rep.zoneNames.Count; i++)
        {
            temp += rep.zoneNames[i] + "\n";
            temp += "Threat Level: " + rep.zoneThreatLevel[i] + "\n";
            temp += "Position: " + rep.zonePosition[i] + "\n";
            temp += "Number of Guards: " + rep.Number_Guards[i] + "\n";
            temp += "Times Assailant passed zone: " + rep.Assailant_pass_throughs[i] + "\n";
            temp += "Number of Windows: " + rep.windows[i] + "\n";
            temp += "Number of Doors: " + rep.doors[i] + "\n";
            temp += "\n";
            if (i == amount)
            {
                Report_Panel_Text.text = temp;
                temp = "";
            }
            else if (i == (amount*2))
            {
                Report_Panel_Text2.text = temp;
                temp = "";
            }
            else if (i == amount*3)
            {
                Report_Panel_Text3.text = temp;
                temp = "";
            }
            else if (i == amount*4)
            {
                Report_Panel_Text4.text = temp;
                temp = "";
            }
        }
        System.IO.File.WriteAllText(Application.dataPath + "/" + "read-able-report.txt", temp);
        Report_Panel.SetActive(true);
        //Report_Panel_Text.text = temp;
    }*/


    /*public void toggleOnOff()
    {
        if (onOff)
        {
            Report_Panel_Text.text = " ";
            Report_Panel.SetActive(false);
        }
        else
        {
            Report_Panel.SetActive(true);
        }
    }*/


}
