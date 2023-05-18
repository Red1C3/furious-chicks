using UnityEngine;
using Unity.Mathematics;

public class FaceClipTest : MonoBehaviour
{
    public BoxCullider box;
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for(int i=0;i<vertices.Length;i++){
            vertices[i]=transform.TransformPoint(vertices[i]);
        }
        Face face = new Face(vertices, true);
        Matrix4x4 a=box.facesMats[0];
        //face=face.clip(a);

        vertices=face.getVertices();
        for(int i=0;i<vertices.Length;i++){
            vertices[i]=transform.InverseTransformPoint(vertices[i]);
            Debug.Log(vertices[i]);
        }
        mesh.vertices = vertices;
        //mesh.triangles=new []{0,1,2,0,2,3};
    }
}