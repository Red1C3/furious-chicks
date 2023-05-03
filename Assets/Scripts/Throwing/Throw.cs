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

    bool press = false, fired = false;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        cam.transform.position = new Vector3(0, 0, cam.transform.position.z - force);
        transform.position = new Vector3(0, 0, -force);

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

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
            if (press)
            {
                transform.position = worldPosition;
                lineRenderer.SetPosition(1, transform.position);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                fired = true;
                rb.useGravity = true;
                Vector3 distance = -transform.position;
                float magnitude = distance.magnitude;
                Vector3 direction = distance.normalized;
                Vector3 Force = direction * magnitude * rb.mass;
                GetComponent<RigidbodyDriver>().addForce(RigidbodyDriver.gravity, ForceMode.Force);
                GetComponent<RigidbodyDriver>().addForce(Force * 500, ForceMode.Impulse);
                lineRenderer.enabled = false;
            }
        }
    }
}
