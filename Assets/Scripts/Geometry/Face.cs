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
            edges=new Edge[0];
            winding=Winding.CCW;
        }

    }

    public Face clip(Transform transform){
        if(winding==Winding.CCW){
            Debug.Log("Can't clip CCW winded faces");
            return this;
        }
        Edge[] clippingArea=new Edge[4]; //Can be computed once by making it static
        clippingArea[0]=new Edge(new Vector2(-1,-1),new Vector2(-1,1));// Must be CW
        clippingArea[1]=new Edge(new Vector2(-1,1),new Vector2(1,1));
        clippingArea[2]=new Edge(new Vector2(1,1),new Vector2(1,-1));
        clippingArea[3]=new Edge(new Vector2(1,-1),new Vector2(-1,-1));

        Face clipped=this; //A copy
        clipped.edges=(Edge[])edges.Clone();

        clipped.toLocal(transform);


        return clipped;//TODO
    }

    public void toLocal(Transform transform){
        for(int i=0;i<edges.Length;i++){
            edges[i].toLocal(transform);
        }
    }
}