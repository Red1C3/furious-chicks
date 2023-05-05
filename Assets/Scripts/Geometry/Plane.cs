using UnityEngine;

public class Plane
{
    public static bool linePlaneIntersection(out Vector3 contactPoint, Vector3 direction,
                                             Vector3 lineOrigin, Vector3 planeNorm, Vector3 planePoint)
    {
        contactPoint = Vector3.zero;
        direction=direction.normalized;
        
        float d = Vector3.Dot(planeNorm, planePoint);

        if (Vector3.Dot(planeNorm, direction) == 0)
        {
            return false;
        }

        float x = (d - Vector3.Dot(planeNorm, lineOrigin)) / Vector3.Dot(planeNorm, direction);

        contactPoint = lineOrigin + direction.normalized * x;
        return true;
    }
}