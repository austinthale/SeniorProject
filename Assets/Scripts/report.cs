using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class report {


    public List<GameObject> Zones = new List<GameObject>();
    public List<string> zoneNames = new List<string>();
    public List<Vector3> zonePosition = new List<Vector3>();
    public List<float> zoneThreatLevel = new List<float>();
    public List<float> Number_Guards = new List<float>();
    public List<float> Assailant_pass_throughs = new List<float>();
    public List<float> windows = new List<float>();
    public List<float> doors = new List<float>();

}
