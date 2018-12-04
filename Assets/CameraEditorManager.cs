using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEditorManager : EditorManager {

    private List<GameObject> camera_list;
    public GameObject camera_prefab;
    private int index = 0;

    public override void Add()
    {
        if (camera_prefab != null)
        {
            camera_list.Add(Instantiate(camera_prefab));
        }
        else
        {
            print("camer_prefab is null:::CameraEditorManager--Add()");
        }
        throw new NotImplementedException();
    }

    public override void Delete(int ind)
    {
        camera_list.Remove(camera_list[ind].gameObject);
        throw new NotImplementedException();
    }

    public override void Move()
    {
        
        throw new NotImplementedException();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
