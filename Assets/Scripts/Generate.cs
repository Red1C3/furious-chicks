using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public GameObject ground;
    public GameObject[] cubes;
    public int size;
    public bool shrink=true,dynamicStart=false;
    // Start is called before the first frame update
    void Awake()
    {
        ground.transform.localScale = new Vector3(size,1,size);
        if(shrink){
            cubes[0].transform.position = new Vector3(size/2.0f-0.5f  ,-0.5f , size/2.0f-0.5f);
            cubes[1].transform.position = new Vector3(size/2.0f-0.5f  ,-0.5f ,-size/2.0f+0.5f);
            cubes[2].transform.position = new Vector3(-size/2.0f+0.5f ,-0.5f , size/2.0f-0.5f);
            cubes[3].transform.position = new Vector3(-size/2.0f+0.5f ,-0.5f ,-size/2.0f+0.5f);

            cubes[3].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3( size/10.0f+0.2f ,0,  size/10.0f+0.2f);
            cubes[2].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3( size/10.0f+0.2f ,0, -size/10.0f-0.2f);
            cubes[1].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(-size/10.0f-0.2f ,0,  size/10.0f+0.2f);
            cubes[0].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(-size/10.0f-0.2f ,0, -size/10.0f-0.2f);
        }
        else
        {
            cubes[0].transform.position = new Vector3(+0.5f ,-0.5f ,+0.5f);
            cubes[1].transform.position = new Vector3(+0.5f ,-0.5f ,-0.5f);
            cubes[2].transform.position = new Vector3(-0.5f ,-0.5f ,+0.5f);
            cubes[3].transform.position = new Vector3(-0.5f ,-0.5f ,-0.5f);

            cubes[3].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(-size/10.0f-0.2f ,0, -size/10.0f-0.2f);
            cubes[2].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(-size/10.0f-0.2f ,0, +size/10.0f+0.2f);
            cubes[1].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(+size/10.0f+0.2f ,0, -size/10.0f-0.5f);
            cubes[0].GetComponent<RigidbodyDriver>().initialVelocity = new Vector3(+size/10.0f+0.2f ,0, +size/10.0f+0.2f);
        
        }
        if(dynamicStart)
            Engine.nodeMinSize=0;
        else if(shrink)
            Engine.nodeMinSize=100;
        else
            Engine.nodeMinSize=1;
    }


}
