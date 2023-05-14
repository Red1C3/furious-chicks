using Unity.Mathematics;
using UnityEngine;

public class VoxelCullider : BoxCullider
{

    private Voxel voxel;
    protected override void Start()
    {
        facesMats = new Matrix4x4[6];
        edges = new Edge[12];
        voxel = GetComponent<Voxel>();
        updateBoundaries();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override float3x3 getTensorInertia()
    {
        return float3x3.identity;
        return base.getTensorInertia(); //TODO return voxel grid inertia tensor
    }

    public override Vector3 center()
    {
        return base.center(); //TODO return voxel grid center
    }

    public override RigidbodyDriver getRigidbodyDriver()
    {
        return voxel.grid.GetComponent<RigidbodyDriver>();
    }

}