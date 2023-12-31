using UnityEngine;

public class Plane
{
    public static bool linePlaneIntersection(out Vector3 contactPoint, Vector3 direction,
                                             Vector3 lineOrigin, Vector3 planeNorm, Vector3 planePoint)
    {
        contactPoint = Vector3.zero;
        direction = direction.normalized;

        float d = Vector3.Dot(planeNorm, planePoint);

        if (Vector3.Dot(planeNorm, direction) == 0)
        {
            return false;
        }

        float x = (d - Vector3.Dot(planeNorm, lineOrigin)) / Vector3.Dot(planeNorm, direction);

        contactPoint = lineOrigin + direction.normalized * x;
        return true;
    }

    public static bool edgePlaneIntersection(out Vector3 contactPoint, Edge edge, Vector3 planeNorm, Vector3 planePoint)
    {
        contactPoint = Vector3.zero;
        Vector3 edgeVec = edge.vec();
        Vector3 direction = edgeVec.normalized;

        float d = Vector3.Dot(planeNorm, planePoint);

        float planeNormDirectionDot = Vector3.Dot(planeNorm, direction);

        if (planeNormDirectionDot == 0)
        {
            return false;
        }

        Vector3 edgeOrigin = edge.from;

        float x = (d - Vector3.Dot(planeNorm, edgeOrigin)) / planeNormDirectionDot;

        contactPoint = edgeOrigin + direction * x;

        if (x > edgeVec.magnitude || x < 0)
        {
            return false;
        }
        return true;
    }
}