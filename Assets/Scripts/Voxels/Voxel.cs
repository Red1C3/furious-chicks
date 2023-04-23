using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public enum Type{NEUTRAL,SURFACE,INTERIOR,EXTERIOR}

    public Type type=Type.NEUTRAL;

    public Vector3Int coords;

    public VoxelGrid grid;

    public Rigidbody rb{get;private set;}

    private List<SpringJoint> springs;

    //gets called from voxelgrid
    public void init(){
        springs=new List<SpringJoint>();
        rb=GetComponent<Rigidbody>();
    }

    public bool isConnected(Voxel other){
        foreach(SpringJoint spring in springs){
            if(spring.connectedBody==other.rb) return true;
        }
        foreach(SpringJoint spring in other.springs){
            if(spring.connectedBody==rb) return true;
        }
        return false;
    }

    public void addSpring(SpringJoint spring){
        springs.Add(spring);
    }

    public override string ToString()
    {
        return "Voxel("+coords.x+","+coords.y+","+coords.z+")";
    }
}
