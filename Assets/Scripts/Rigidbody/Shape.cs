using UnityEngine;
using Unity.Mathematics;

public interface Shape
{
    float3x3 getTensorInertia();
    Vector3 center();
    static float inertiaScalar(float3x3 inertiaTensor, float3 axis)
    {
        if (math.all(axis == float3.zero))
        {
            return math.EPSILON;
        }
        axis = math.normalize(axis);
        return math.mul(math.mul(axis, inertiaTensor), axis);
    }
}