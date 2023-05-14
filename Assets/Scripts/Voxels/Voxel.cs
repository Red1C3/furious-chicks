using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public enum Type{NEUTRAL,SURFACE,INTERIOR,EXTERIOR}

    public Type type=Type.NEUTRAL;

    public Vector3Int coords;

    public VoxelGrid grid;
    //gets called from voxelgrid
    public void init(){
    }

    public override string ToString()
    {
        return "Voxel("+coords.x+","+coords.y+","+coords.z+")";
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
}
