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

    public GameObject trigger;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            times_assialant_passed_through++;
        }
        if (other.gameObject.layer == 11 || other.gameObject.layer == 12)
        {
            number_guards++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            number_guards--;
        }
    }
}
