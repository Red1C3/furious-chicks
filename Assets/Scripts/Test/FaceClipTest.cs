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
        face = face.clip(clippingPlane);
        vertices=face.getVertices();
        for(int i=0;i<vertices.Length;i++){
            vertices[i]=transform.InverseTransformPoint(vertices[i]);
        }
        mesh.vertices = vertices;
    }
}