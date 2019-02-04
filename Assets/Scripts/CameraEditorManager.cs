using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEditorManager : EditorManager {

    private List<GameObject> camera_list;
    public GameObject camera_prefab;
    private int index;


    // Use this for initialization
    void Start()
    {
        camera_list = new List<GameObject>();
        index = 0;
    }

    // Use for Adding Cameras to the scene 
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
        //throw new NotImplementedException();
    }

    public override void Delete(int ind)
    {
        camera_list.Remove(camera_list[ind].gameObject);
        //throw new NotImplementedException();
    }

    public override void Move()
    {
        
        throw new NotImplementedException();
    }


    public int GetSize()
    {
        return camera_list.Count;
    }

}
