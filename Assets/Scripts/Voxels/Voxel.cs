using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public enum Type { NEUTRAL = 1, SURFACE = 2, INTERIOR = 4, EXTERIOR = 8 }

    public Type type;

    public Vector3Int coords;

    public VoxelGrid grid;
    //gets called from voxelgrid
    public void init(bool display)
    {
        type = Type.NEUTRAL;
        GetComponent<MeshRenderer>().enabled = display;
    }

    public override string ToString()
    {
        return "Voxel(" + coords.x + "," + coords.y + "," + coords.z + ")";
    }
    public float3x3 getInertiaTensor(float mass){
        float3x3 tensor = float3x3.identity;
        float h = transform.lossyScale.y;
        float d = transform.lossyScale.z;
        float w = transform.lossyScale.x;

        mass = mass / 12.0f;

        tensor[0][0] = mass * (h * h + d * d);
        tensor[1][1] = mass * (w * w + d * d);
        tensor[2][2] = mass * (w * w + h * h);

        return tensor;
    }
    public Bounds getBounds(){
        return GetComponent<Renderer>().bounds;
    }
}
