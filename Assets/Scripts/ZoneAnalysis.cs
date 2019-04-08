using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneAnalysis : MonoBehaviour {

    public string ZoneName = "zone";
    public float threatLevel = 100;
    public float number_guards = 0;
    public float times_assialant_passed_through = 0;
    public float windows = 0;
    public float doors = 0;
    public float walls = 0;

    public GameObject trigger;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<zoneTag>())
        {
            if (other.GetComponent<zoneTag>().Type == zoneTag.tag.assailant)
            {
                times_assialant_passed_through++;
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.guard)
            {
                number_guards++;
                other.GetComponent<zoneTag>().activeZones.Add(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.wall)
            {
                walls++;
                other.GetComponent<zoneTag>().activeZones.Add(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.door)
            {
                doors++;
                other.GetComponent<zoneTag>().activeZones.Add(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.window)
            {
                windows++;
                other.GetComponent<zoneTag>().activeZones.Add(this.gameObject);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<zoneTag>())
        {
            if (other.GetComponent<zoneTag>().Type == zoneTag.tag.guard)
            {
                number_guards--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.wall)
            {
                walls--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.door)
            {
                doors--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.window)
            {
                windows--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }

        }
    }

    public void off(GameObject other)
    {
        if (other.GetComponent<zoneTag>())
        {
            if (other.GetComponent<zoneTag>().Type == zoneTag.tag.guard)
            {
                number_guards--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.wall)
            {
                walls--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.door)
            {
                doors--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }
            else if (other.GetComponent<zoneTag>().Type == zoneTag.tag.window)
            {
                windows--;
                other.GetComponent<zoneTag>().activeZones.Remove(this.gameObject);
            }

        }
    }
}
