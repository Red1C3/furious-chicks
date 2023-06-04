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
    public LineRenderer lineRenderer;
    public int cameraAway = 3;

    bool press = false, fired = false;

    private RigidbodyDriver rigidbodyDriver;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, -force);
        cam.transform.position = new Vector3(0, 0, -(force+cameraAway));

        rigidbodyDriver=GetComponent<RigidbodyDriver>();
        rigidbodyDriver.useGravity = false;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, transform.position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!fired)
        {
            if (Input.GetMouseButtonDown(0))
                press = true;
            if (Input.GetKey(KeyCode.Z))
                press = false;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z - force));
            float pzSq = (force*force)-(worldPosition.x*worldPosition.x)-(worldPosition.y*worldPosition.y);
            if (press && pzSq>0)
            {
                float pz = -Mathf.Sqrt(pzSq);
                transform.position = new Vector3(worldPosition.x,worldPosition.y,pz);
                cam.transform.position = new Vector3(worldPosition.x,worldPosition.y,-(force+cameraAway));
                cam.transform.LookAt(Vector3.zero);
                lineRenderer.SetPosition(1, transform.position);
            }
            if(Input.GetKey(KeyCode.UpArrow))
                force-=0.1f;
            else if(Input.GetKey(KeyCode.DownArrow))
                force+=0.1f;
            if (Input.GetKeyDown(KeyCode.E))
            {
                fired = true;
                rigidbodyDriver.useGravity = true;
                Vector3 distance = -transform.position;
                float magnitude = distance.magnitude;
                Vector3 direction = distance.normalized;
                Vector3 Force = direction * magnitude * rigidbodyDriver.mass;
                GetComponent<RigidbodyDriver>().addForce(RigidbodyDriver.gravity, ForceMode.Force);
                GetComponent<RigidbodyDriver>().addForce(Force * 500, ForceMode.Impulse);
                lineRenderer.enabled = false;
            }
        }
    }
    public bool hasFired(){
        return fired;
    }
}
