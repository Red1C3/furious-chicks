using UnityEngine;

public struct Edge
{
    public Vector3 from { get; private set; }
    public Vector3 to { get; private set; }
    public Edge(Vector3 from, Vector3 to)
    {
        this.from = from;
        this.to = to;
    }
    public void toLocal(Transform transform)
    {
        from = transform.InverseTransformPoint(from);
        to = transform.InverseTransformPoint(to);
    }

    public Vector3[] clip(Edge clipper)
    {
        float fromSign = (clipper.to.x - clipper.from.x) * (from.y - clipper.from.y) -
                        (clipper.to.y - clipper.from.y) * (from.x - clipper.from.x);

        float toSign = (clipper.to.x - clipper.from.x) * (to.y - clipper.from.y) -
                        (clipper.to.y - clipper.from.y) * (to.x - clipper.from.x);

        //Both vertices are inside the clipping area
        if (fromSign <= 0 && toSign <= 0)
        {
            return new[] { to };
        }
        else if (toSign <= 0)
        { //Only the 2nd vertex is inside
          //return both the intersection point between the edges and to vertex
            return new Vector3[] { }; //TODO

        }
        else if (fromSign <= 0)
        {
            //return only the point of intersection
            return new Vector3[] { }; //TODO

        }
        else
        {
            return new Vector3[] { };
        }

    }
}