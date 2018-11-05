using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSelection : MonoBehaviour {

    public Material[] materials = new Material[2]; // Material color for when selected and non-selected
    public Renderer rend; // Rendering the grid square
    public GameObject select_zone;

    private bool selected;

	// Use this for initialization
	void Start () {
        selected = false;
        rend = gameObject.GetComponent<Renderer>();
        

	}

    private void OnEnable()
    {
        select_zone = GameObject.FindGameObjectWithTag("User Input Manager");
    }
    private void OnMouseDown()
    { 
        if (!selected)
        {
            if (select_zone.GetComponent<SelectZone>().currentZone != null)
            {
                select_zone.GetComponent<SelectZone>().currentZone.GetComponent<ToggleSelection>().rend.material = materials[0];
                select_zone.GetComponent<SelectZone>().currentZone.GetComponent<ToggleSelection>().selected = false;
            }
            
            print(gameObject.name);
            rend.material = materials[1]; //1 will be index for selected color
            selected = true;
            select_zone.GetComponent<SelectZone>().Zone_Info_Plane.SetActive(true);
            select_zone.GetComponent<SelectZone>().writeZoneInfo(gameObject);
            
        }
            
        else {
            select_zone.GetComponent<SelectZone>().currentZone = null;
            select_zone.GetComponent<SelectZone>().Zone_Info_Plane.SetActive(false);
            rend.material = materials[0];
            selected = false;
            //select_zone.GetComponent<SelectZone>().removeZone(gameObject);
        }
            
    }

}
