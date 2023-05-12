using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public enum Type{NEUTRAL=1,SURFACE=2,INTERIOR=4,EXTERIOR=8}

    public Type type;

    public Vector3Int coords;

    public VoxelGrid grid;
    //gets called from voxelgrid
    public void init(){
        type=Type.NEUTRAL;
    }

    public override string ToString()
    {
        return "Voxel("+coords.x+","+coords.y+","+coords.z+")";
    }
}
