using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zoneTag : MonoBehaviour {
    public enum tag
    {
        assailant,
        guard,
        wall,
        door,
        window,
        camera
    };
    public List<GameObject> activeZones = new List<GameObject>();

    public tag Type;

    public void OnDisable()
    {
        TurnOff();
    }

    public void OnDestroy()
    {
        TurnOff();
    }

    public void TurnOff()
    {
        foreach (GameObject zone in activeZones)
        {
            if (zone.GetComponent<ZoneAnalysis>())
            {
                zone.GetComponent<ZoneAnalysis>().off(this.gameObject);
                
            }
        }
    }

}
