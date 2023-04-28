using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    private static Vector3 gravity=new Vector3(0,-9.8f,0);
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    void FixedUpdate(){
        velocity+=gravity*Time.fixedDeltaTime;
        transform.position+=velocity*Time.fixedDeltaTime;
    }
}
