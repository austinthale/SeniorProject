using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;


public class ReportManager : MonoBehaviour {

    public GameObject GridManage;
    public bool run = false;
    public string ReportName = "report.txt";
    public report rep = new report();
    public GameObject Report_Panel;
    public Text Report_Panel_Text;

    public report createReport(List<GameObject> zones)
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
    }

    private void Update()
    {
        if (run)
        {
            SaveReport();
            run = false;
        }
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
    }

    public void readableReport()
    {
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
        }
        System.IO.File.WriteAllText(Application.dataPath + "/" + "read-able-report.txt", temp);
        Report_Panel.SetActive(true);
        Report_Panel_Text.text = temp;
    }


}
