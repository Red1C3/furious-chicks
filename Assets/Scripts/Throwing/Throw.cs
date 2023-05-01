using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public float force;
    public Camera cam;
    private float angleX;
    private float angleY;
    private float angleZ;

    bool press= false;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        cam.transform.position=new Vector3(0,0,cam.transform.position.z-force);
        transform.position=new Vector3(0,0,-force);

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Input.GetMouseButtonDown(0))
            press=true;
        if(Input.GetKey (KeyCode.Z))
            press=false;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z-force));
        if(press)
            transform.position=worldPosition;
            Debug.Log(worldPosition);

        if (Input.GetKey (KeyCode.E)){
            rb.isKinematic = false;
            Vector3 distance = -transform.position;
            float magnitude = distance.magnitude;
            Vector3 direction = distance.normalized;
            Vector3 Force = direction * magnitude * rb.mass;
            rb.AddForce(Force, ForceMode.Impulse);
        }
    }
}
