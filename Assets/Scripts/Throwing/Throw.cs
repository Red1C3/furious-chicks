using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public float force;
    private float angleX;
    private float angleY;
    private float angleZ;
    public LineRenderer lineRenderer;
    public float cameraAway = 3;

    public bool fired{get; private set;}
    private bool press = false,cameraNotSetOnStart=false;

    private RigidbodyDriver rigidbodyDriver;
    // Start is called before the first frame update
    void Start()
    {
        fired = false; 
        transform.position = new Vector3(0, 0, -force);
        if(LevelCtrlr.playerView)
            LevelCtrlr.cam.transform.position = new Vector3(0, 0, -(force+cameraAway));
        else
            cameraNotSetOnStart=true;

        rigidbodyDriver=GetComponent<RigidbodyDriver>();
        rigidbodyDriver.useGravity = false;

        lineRenderer.SetPosition(0, new Vector3(-1,0,0));
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.SetPosition(2, new Vector3(1,0,0));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (LevelCtrlr.playerView && !fired)
        {
            if (cameraNotSetOnStart){
                LevelCtrlr.cam.transform.position = new Vector3(0, 0, -(force+cameraAway));
                cameraNotSetOnStart=false;
            }
            if (Input.GetMouseButtonDown(0))
                press = true;
            if (Input.GetKey(KeyCode.Z))
                press = false;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -LevelCtrlr.cam.transform.position.z - force));
            float pzSq = (force*force)-(worldPosition.x*worldPosition.x)-(worldPosition.y*worldPosition.y);
            if (press && pzSq>=0)
            {
                float pz = -Mathf.Sqrt(pzSq);
                transform.position = new Vector3(worldPosition.x,worldPosition.y,pz);
                transform.LookAt(Vector3.zero);
                LevelCtrlr.cam.transform.position = new Vector3(worldPosition.x,worldPosition.y,-(force+cameraAway));
                LevelCtrlr.cam.transform.LookAt(Vector3.zero);
                lineRenderer.SetPosition(1, transform.position);
            }
            else if(press && force<=0.01f){
                force=1.0f;
            }
            if(Input.GetKey(KeyCode.UpArrow)){
                force = Mathf.Max(0.1f,force-0.1f);
            }
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
                GetComponent<RigidbodyDriver>().addForce(RigidbodyDriver.gravity*GetComponent<RigidbodyDriver>().mass, ForceMode.Force);
                GetComponent<RigidbodyDriver>().addForce(Force * 500, ForceMode.Impulse);
                lineRenderer.enabled = false;
            }
        }
        else if(fired && LevelCtrlr.playerView){
            LevelCtrlr.playerView=false;
            LevelCtrlr.once=true;
        }
    }
    public bool hasFired(){
        return fired;
    }
}
