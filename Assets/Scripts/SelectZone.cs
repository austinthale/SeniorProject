using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectZone : MonoBehaviour
{
    public Text zoneMenu;
    public string zoneInfo = "";
    public GameObject currentZone;

    //public List<GameObject> Zones = new List<GameObject>();
    public List<string> zoneNames = new List<string>();
    //public List<Vector3> zonePosition = new List<Vector3>();
    public List<float> zoneThreatLevel = new List<float>();
    public List<float> Number_Guards = new List<float>();
    public List<float> Assailant_pass_throughs = new List<float>();
    public List<float> windows = new List<float>();
    public List<float> doors = new List<float>();

    public GameObject Zone_Info_Plane;
    public Slider threat_level_slider;
    public Text threat_level_number;
    public Text guard_number;
    public Text window_number;
    public Text door_amount;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void addZone(GameObject passed_zone)
    {
        zoneNames.Add(passed_zone.GetComponent<ZoneAnalysis>().ZoneName);
        zoneThreatLevel.Add(passed_zone.GetComponent<ZoneAnalysis>().threatLevel);
        Number_Guards.Add(passed_zone.GetComponent<ZoneAnalysis>().number_guards);
        Assailant_pass_throughs.Add(passed_zone.GetComponent<ZoneAnalysis>().times_assialant_passed_through);
        windows.Add(passed_zone.GetComponent<ZoneAnalysis>().windows);
        doors.Add(passed_zone.GetComponent<ZoneAnalysis>().doors);
        //writeZoneInfo();
    }


    public void removeZone(GameObject passed_zone)
    {
        for (int i = 0; i < zoneNames.Count; i++)
        {
            if (passed_zone.GetComponent<ZoneAnalysis>().ZoneName == zoneNames[i])
            {
                zoneNames.RemoveAt(i);
                zoneThreatLevel.RemoveAt(i);
                Number_Guards.RemoveAt(i);
                Assailant_pass_throughs.RemoveAt(i);
                windows.RemoveAt(i);
                doors.RemoveAt(i);
            }
        }
        //writeZoneInfo();
    }

    public void writeZoneInfo(GameObject passed_zone)
    {
        currentZone = passed_zone;
        zoneMenu.text = passed_zone.GetComponent<ZoneAnalysis>().ZoneName;
        threat_level_slider.value = currentZone.GetComponent<ZoneAnalysis>().threatLevel;
        threat_level_number.text = currentZone.GetComponent<ZoneAnalysis>().threatLevel.ToString();
        guard_number.text = currentZone.GetComponent<ZoneAnalysis>().number_guards.ToString();
        window_number.text = currentZone.GetComponent<ZoneAnalysis>().windows.ToString();
        door_amount.text = currentZone.GetComponent<ZoneAnalysis>().doors.ToString();
    }

    public void adjustZoneThreatLevel()
    {
        currentZone.GetComponent<ZoneAnalysis>().threatLevel = threat_level_slider.value;
        writeZoneInfo(currentZone);
    }
}
