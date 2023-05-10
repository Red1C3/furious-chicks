using System.Collections;
using System.Collections.Generic;
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
}
