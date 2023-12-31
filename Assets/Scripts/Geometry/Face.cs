using UnityEngine;
using System.Collections.Generic;

public struct Face
{
    public enum Winding { CW, CCW }
    private Edge[] edges;
    public Winding winding { get; set; }
    private static Edge[] clippingArea = new Edge[]{
        new Edge(new Vector3(-1, 0, -1), new Vector3(-1, 0, 1)),
        new Edge(new Vector3(-1, 0, 1), new Vector3(1, 0, 1)),
        new Edge(new Vector3(1, 0, 1), new Vector3(1, 0, -1)),
        new Edge(new Vector3(1, 0, -1), new Vector3(-1, 0, -1))};

    private static Vector3[] clippingNorms = new Vector3[]{
        new Vector3(1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1)};

    private static List<Vector3> vertList = new List<Vector3>(24);
    private static List<Vector3> projectedList = new List<Vector3>(24);
    public Face(Vector3[] vertices, bool flipWinding)
    {
        if (vertices.Length == 3)
        {
            edges = new Edge[3];
            edges[0] = new Edge(vertices[0], vertices[1]);
            edges[1] = new Edge(vertices[1], vertices[2]);
            edges[2] = new Edge(vertices[2], vertices[0]);

            if (flipWinding) //Unity's default is CCW
            {
                Edge tempEdge = edges[0];
                edges[0] = edges[2];
                edges[2] = tempEdge;
                winding = Winding.CW;
            }
            else
            {
                winding = Winding.CCW;
            }
        }
        else
        {
            Debug.Log("Non supported number of vertices was passed to create a faces");
            edges = new Edge[0];
            winding = Winding.CCW;
        }

    }

    public Face(Matrix4x4 faceMat)
    {
        winding = Winding.CCW;
        edges = new Edge[4];
        Vector3[] vertices = new Vector3[4];
        vertices[0] = faceMat * (new Vector4(-1, 0, -1, 1));
        vertices[1] = faceMat * (new Vector4(1, 0, -1, 1));
        vertices[2] = faceMat * (new Vector4(-1, 0, 1, 1));
        vertices[3] = faceMat * (new Vector4(1, 0, 1, 1));

        edges[0] = new Edge(vertices[0], vertices[2]);
        edges[1] = new Edge(vertices[2], vertices[3]);
        edges[2] = new Edge(vertices[3], vertices[1]);
        edges[3] = new Edge(vertices[1], vertices[0]);

    }


    private Face(Vector3[] vertices)
    {
        winding = Winding.CW;
        edges = new Edge[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            edges[i] = new Edge(vertices[i], vertices[(i + 1) % vertices.Length]);
        }
    }

    public Vector3[] clip(Matrix4x4 mat)
    {
        if (winding == Winding.CCW)
        {
            Debug.Log("Can't clip CCW winded faces");
            return null;
        }

        Face clipped = this; //A copy
        clipped.edges = (Edge[])edges.Clone();

        clipped.toLocal(mat);


        vertList.Clear();
        foreach (Edge edge in clipped.edges)
        {
            vertList.Add(edge.from);
        }

        for (int i = 0; i < clippingArea.Length; i++)
        {
            List<Vector3> newList = new List<Vector3>(vertList.Count);
            for (int j = 0; j < vertList.Count; j++)
            {
                Edge edge = new Edge(vertList[j], vertList[(j + 1) % vertList.Count]);
                newList.AddRange(edge.clip(clippingArea[i], clippingNorms[i]));
            }
            vertList = newList;
        }

        projectedList.Clear();
        for (int i = 0; i < vertList.Count; i++)
        {
            if (vertList[i].y <= 0)
            {
                vertList[i] = Vector3.ProjectOnPlane(vertList[i], new Vector3(0, 1, 0));
                vertList[i] = mat * (new Vector4(vertList[i].x, vertList[i].y, vertList[i].z, 1));
                projectedList.Add(vertList[i]);
            }
        }

        return (projectedList.ToArray());
    }

    public void toLocal(Matrix4x4 mat)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            edges[i].toLocal(mat);
        }
    }
    public Vector3[] getVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (Edge e in edges)
        {
            vertices.Add(e.from);
        }
        return vertices.ToArray();
    }

    public void flip()
    {
        if (edges.Length == 4)
        {
            Edge tempEdge = edges[1];
            edges[1] = edges[3];
            edges[3] = tempEdge;
            edges[0].flip();
            edges[2].flip();
            edges[1].flip();
            edges[3].flip();
            if (winding == Winding.CW) winding = Winding.CCW;
            else winding = Winding.CW;
        }
        else
        {
            Debug.Log("Face flip was called with unsupported face");
        }
    }

    public static Vector3 normal(Matrix4x4 mat)
    {
        Vector3 norm = mat * Vector3.up;
        return norm.normalized;
    }
}