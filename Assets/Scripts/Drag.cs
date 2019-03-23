using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour {

    public float dragSpeed = 2;
    private Vector3 mouseOffset;
    private float mZCord;


    private void OnMouseDown()
    {
        mZCord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        mouseOffset = gameObject.transform.position - getMousePosition();
    }

    private Vector3 getMousePosition()
    {
        

        Vector3 mousePoint = Input.mousePosition;

        mousePoint.z = mZCord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        float tempx = (getMousePosition().x + mouseOffset.x);
        float tempz = (getMousePosition().z + mouseOffset.z);

        transform.position = new Vector3(tempx, 0, tempz);
    }





    //old way
    /*void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * -dragSpeed, 0, pos.y * -dragSpeed);

        transform.Translate(move, Space.World);
    }*/
}
