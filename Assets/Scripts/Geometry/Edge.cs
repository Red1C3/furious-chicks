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
    public void toLocal(Matrix4x4 mat)
    {
        //from = transform.InverseTransformPoint(from);
        //to = transform.InverseTransformPoint(to);
        from = mat.inverse * (new Vector4(from.x, from.y, from.z, 1));
        to = mat.inverse * (new Vector4(to.x, to.y, to.z, 1));
    }

    public Vector3[] clip(Edge clipper, Vector3 clippingPlaneNorm)
    {
        float fromSign = (clipper.to.x - clipper.from.x) * (from.z - clipper.from.z) -
                        (clipper.to.z - clipper.from.z) * (from.x - clipper.from.x);

        float toSign = (clipper.to.x - clipper.from.x) * (to.z - clipper.from.z) -
                        (clipper.to.z - clipper.from.z) * (to.x - clipper.from.x);


        //Both vertices are inside the clipping area
        if (fromSign <= 0 && toSign <= 0)
        {
            return new[] { to };
        }
        else if (toSign <= 0)
        { //Only the 2nd vertex is inside
          //return both the intersection point between the edges and to vertex
            Vector3 intersectionPoint;
            Plane.linePlaneIntersection(out intersectionPoint, to - from, from,
                                        clippingPlaneNorm, clipper.from);
            return new Vector3[] { intersectionPoint, to };

        }
        else if (fromSign <= 0)
        {
            //return only the point of intersection
            Vector3 intersectionPoint;
            Plane.linePlaneIntersection(out intersectionPoint, to - from, from,
                                        clippingPlaneNorm, clipper.from);
            return new Vector3[] { intersectionPoint };

        }
        else
        {
            return new Vector3[] { };
        }

    }
    public void flip(){
        Vector3 tempVert=from;
        from=to;
        to=tempVert;
    }
}