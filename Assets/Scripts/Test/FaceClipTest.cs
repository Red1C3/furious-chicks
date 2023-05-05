using UnityEngine;

public class FaceClipTest : MonoBehaviour
{
    public Transform clippingPlane;
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for(int i=0;i<vertices.Length;i++){
            vertices[i]=transform.TransformPoint(vertices[i]);
        }
        Face face = new Face(vertices, true);
        //face = face.clip(Matrix4x4.TRS(clippingPlane.position,clippingPlane.rotation,clippingPlane.localScale/2.0f));
        face=face.clip(Matrix4x4.Translate(clippingPlane.position)*Matrix4x4.Rotate(clippingPlane.rotation)*Matrix4x4.Scale(clippingPlane.localScale/2.0f));
        vertices=face.getVertices();
        for(int i=0;i<vertices.Length;i++){
            vertices[i]=transform.InverseTransformPoint(vertices[i]);
            //Debug.Log(vertices[i]);
        }
        mesh.vertices = vertices;
        //mesh.triangles=new []{0,1,2,0,2,3};
    }
}