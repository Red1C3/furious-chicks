using UnityEngine;
using Unity.Mathematics;

public interface Shape{
    float3x3 getTensorInertia();
    Vector3 center();
}