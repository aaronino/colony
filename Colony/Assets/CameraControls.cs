using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float dragSpeed = 4;
    public float zoomSpeed = .1f;
    private Vector3 dragOrigin;
    private float currentScroll;
    
 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
 
        if (Input.GetMouseButton(0)) {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
    
            transform.Translate(move, Space.World);  
        }

        if (Input.mouseScrollDelta.y != 0)
            GetComponent<Camera>().orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
    }


    // Start is called before the first frame update
    void Start()
    {
        currentScroll = Input.GetAxis("Mouse ScrollWheel");
    }

}
