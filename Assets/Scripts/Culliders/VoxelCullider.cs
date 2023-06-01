using System.Collections.Generic;
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
        return voxel.grid.getInertiaTensor();
    }

    public override Vector3 center()
    {
        return voxel.grid.getVoxelsCenter();
    }

    public override RigidbodyDriver getRigidbodyDriver()
    {
        return voxel.grid.GetComponent<RigidbodyDriver>();
    }
    public void setFriction(float friction) //TODO setBounciness
    {
        frictionCo = friction;
    }
    public override HashSet<Cullider> getFrameCulliders()
    {
        return voxel.grid.frameCulliders;
    }
    public override HashSet<Cullider> getStayedCulliders()
    {
        return voxel.grid.stayedCulliders;
    }
}