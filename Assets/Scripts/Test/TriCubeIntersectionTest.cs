using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriCubeIntersectionTest : MonoBehaviour
{
    public Transform cube;
    private Vector3[] vertices;
    void Start()
    {
        vertices=GetComponent<MeshFilter>().mesh.vertices;
        Debug.Log(vertices.Length);
    }

    void Update()
    {
        Vector3[] transformedVertices=new Vector3[3];
        for(int i=0;i<3;i++) transformedVertices[i]=transform.TransformPoint(vertices[i]);
        ulong res=TriCubeIntersection.triCubeIntersection(transformedVertices,cube);
        if (res==0) Debug.Log("is intersecting");
        else Debug.Log("NOT intersecting");
    }
}
