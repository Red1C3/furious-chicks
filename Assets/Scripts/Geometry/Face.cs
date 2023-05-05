using UnityEngine;
using System.Collections.Generic;

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

    private Face(Vector3[] vertices)
    {
        winding = Winding.CW;
        edges = new Edge[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            edges[i] = new Edge(vertices[i], vertices[(i + 1) % vertices.Length]);
        }
    }

    public Face clip(Transform transform)
    {
        if (winding == Winding.CCW)
        {
            Debug.Log("Can't clip CCW winded faces");
            return this;
        }
        Edge[] clippingArea = new Edge[4]; //Can be computed once by making it static
        clippingArea[0] = new Edge(new Vector3(-1, 0, -1), new Vector3(-1, 0, 1));// Must be CW
        clippingArea[1] = new Edge(new Vector3(-1, 0, 1), new Vector3(1, 0, 1));
        clippingArea[2] = new Edge(new Vector3(1, 0, 1), new Vector3(1, 0, -1));
        clippingArea[3] = new Edge(new Vector3(1, 0, -1), new Vector3(-1, 0, -1));

        Vector3[] clippingNorms = new Vector3[4];
        clippingNorms[0] = new Vector3(1, 0, 0);
        clippingNorms[1] = new Vector3(0, 0, -1);
        clippingNorms[2] = new Vector3(-1, 0, 0);
        clippingNorms[3] = new Vector3(0, 0, 1);

        Face clipped = this; //A copy
        clipped.edges = (Edge[])edges.Clone();

        clipped.toLocal(transform);


        List<Vector3> vertList = new List<Vector3>();
        foreach (Edge edge in edges)
        {
            vertList.Add(edge.from);
        }

        for (int i = 0; i < clippingArea.Length; i++)
        {
            List<Vector3> newList = new List<Vector3>();
            for (int j = 0; j < vertList.Count; j++)
            {
                Edge edge = new Edge(vertList[j], vertList[(j + 1) % vertList.Count]);
                newList.AddRange(edge.clip(clippingArea[i], clippingNorms[i]));
            }
            vertList = newList;
        }

        return new Face(vertList.ToArray());
    }

    public void toLocal(Transform transform)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            edges[i].toLocal(transform);
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
}