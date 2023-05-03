using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshReadTest : MonoBehaviour
{
    MeshFilter meshFilter;
    void Start()
    {
        meshFilter=GetComponent<MeshFilter>();
        Vector3[] vertices=meshFilter.mesh.vertices;
        foreach(Vector3 v in vertices){
            Debug.Log(v);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
