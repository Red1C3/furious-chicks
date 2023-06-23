using UnityEngine;
using System.Collections.Generic;

public struct Edge
{
    public Vector3 from { get; private set; }
    public Vector3 to { get; private set; }
    private static List<Vector3> clipTemp = new List<Vector3>(2);
    public Edge(Vector3 from, Vector3 to)
    {
        this.from = from;
        this.to = to;
    }
    public void toLocal(Matrix4x4 mat)
    {
        from = mat.inverse * (new Vector4(from.x, from.y, from.z, 1));
        to = mat.inverse * (new Vector4(to.x, to.y, to.z, 1));
    }

    public List<Vector3> clip(Edge clipper, Vector3 clippingPlaneNorm)
    {
        clipTemp.Clear();

        float fromSign = (clipper.to.x - clipper.from.x) * (from.z - clipper.from.z) -
                        (clipper.to.z - clipper.from.z) * (from.x - clipper.from.x);

        float toSign = (clipper.to.x - clipper.from.x) * (to.z - clipper.from.z) -
                        (clipper.to.z - clipper.from.z) * (to.x - clipper.from.x);


        //Both vertices are inside the clipping area
        if (fromSign <= 0 && toSign <= 0)
        {
            clipTemp.Add(to);
            return clipTemp;
        }
        else if (toSign <= 0)
        { //Only the 2nd vertex is inside
          //return both the intersection point between the edges and to vertex
            Vector3 intersectionPoint;
            Plane.linePlaneIntersection(out intersectionPoint, to - from, from,
                                        clippingPlaneNorm, clipper.from);
            clipTemp.Add(intersectionPoint);
            clipTemp.Add(to);
            return clipTemp;
        }
        else if (fromSign <= 0)
        {
            //return only the point of intersection
            Vector3 intersectionPoint;
            Plane.linePlaneIntersection(out intersectionPoint, to - from, from,
                                        clippingPlaneNorm, clipper.from);
            clipTemp.Add(intersectionPoint);
            return clipTemp;
        }
        else
        {
            return clipTemp;
        }

    }
    public void flip()
    {
        Vector3 tempVert = from;
        from = to;
        to = tempVert;
    }
    public Vector3 closestPoint(Edge other)
    {
        Vector3 point = from;
        Vector3 otherPoint = other.from;
        Vector3 norm = (to - from).normalized;
        Vector3 otherNorm = (other.to - other.from).normalized;

        var pos_diff = point - otherPoint;
        var cross_normal = Vector3.Cross(norm, otherNorm).normalized;
        var rejection = pos_diff - Vector3.Project(pos_diff, otherNorm) - Vector3.Project(pos_diff, cross_normal);
        var distance_to_line_pos = rejection.magnitude / Vector3.Dot(norm, rejection.normalized);
        distance_to_line_pos = Mathf.Clamp(distance_to_line_pos, -vec().magnitude, 0);
        var closest_approach = point - norm * distance_to_line_pos;
        return closest_approach;
    }
    public Vector3 vec()
    {
        return (to - from);
    }
    public override string ToString()
    {
        return "Edge:" + from.ToString() + " -> " + to.ToString();
    }
}