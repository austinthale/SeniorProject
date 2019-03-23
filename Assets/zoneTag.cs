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
    public tag Type;
}
