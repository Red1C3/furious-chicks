using Unity.Mathematics;
using UnityEngine;

public class VoxelCullider: BoxCullider{

    private Voxel voxel;
    protected override void Start(){
        base.Start();
        voxel=GetComponent<Voxel>();
    }
    protected override void FixedUpdate(){
        base.FixedUpdate();
    }

    public override float3x3 getTensorInertia(){
        return base.getTensorInertia(); //TODO return voxel grid inertia tensor
    }

    //TODO implement shape functions, and box cullider functions other than cullide with
}