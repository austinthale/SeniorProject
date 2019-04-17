using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropEditor : MonoBehaviour {

    public enum propType
    {
        chair,
        desk,
        plant
    };
    public propType type;
    public int index;
}
