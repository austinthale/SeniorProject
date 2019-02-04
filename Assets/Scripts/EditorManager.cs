using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EditorManager : MonoBehaviour {

    public abstract void Add();
    public abstract void Move();
    public abstract void Delete(int ind);
    private string _name;
}
