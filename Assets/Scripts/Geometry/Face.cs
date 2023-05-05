using UnityEngine;

public struct Face
{
    public enum Winding { CW, CCW }
    private Edge[] edges;
    public Winding winding { get; private set; }
    public Face(Vector3[] vertices, bool flipWinding)
    {
        if (vertices.Length == 3)
        {
            edges = new Edge[3];
            edges[0] = new Edge(vertices[0], vertices[1]);
            edges[1] = new Edge(vertices[1], vertices[2]);
            edges[2] = new Edge(vertices[2], vertices[0]);
            
            if (flipWinding)
            {
                Edge tempEdge = edges[0];
                edges[0] = edges[2];
                edges[2] = tempEdge;
                winding = CW;
            }
            else
            {
                winding = Winding.CCW;
            }
        }
        else
        {
            Debug.Log("Non supported number of vertices was passed to create a faces");
        }

    }
}