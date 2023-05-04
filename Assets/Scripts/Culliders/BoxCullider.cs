using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoxCullider : MonoBehaviour, Cullider
{
    private Vector3 center;
    private Vector3 size;
    private Quaternion rotation;

    private Vector3[] vertices;
    private Vector3 right, up, forward;

    public static readonly float axisThreshold = 0.01f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        updateBoundaries();
    }

    void FixedUpdate()
    {
        updateBoundaries();
    }

    public Rigidbody getRigidbody()
    {
        Voxel voxel;
        if (TryGetComponent<Voxel>(out voxel))
        {
            return voxel.grid.GetComponent<Rigidbody>();
        }
        return rb;
    }
    public Bounds getBounds()
    {
        //REQUIRES mesh,otherwise use the commented code
        return GetComponent<Renderer>().bounds;
        /*float minX,maxX,minY,maxY,minZ,maxZ;
        minX=minY=minZ=float.MaxValue;
        maxX=maxY=maxZ=float.MinValue;
        for(int i=0;i<vertices.Length;i++){
            if(vertices[i].x<minX) minX=vertices[i].x;
            if(vertices[i].x>maxX) maxX=vertices[i].x;
            if(vertices[i].y<minY) minY=vertices[i].y;
            if(vertices[i].y>maxY) maxY=vertices[i].y;
            if(vertices[i].z<minZ) minZ=vertices[i].z;
            if(vertices[i].z>maxZ) maxZ=vertices[i].z;
        }

        Bounds bounds=new Bounds();
        bounds.center=new Vector3((minX+maxX)/2.0f,(minY+maxY)/2.0f,(minZ+maxZ)/2.0f);
        bounds.extents=(new Vector3(maxX,maxY,maxZ))-bounds.center;
        return bounds;*/
    }

    public CullisionInfo cullideWith(Cullider other)
    {
        CullisionInfo cullisionInfo;
        if (other is BoxCullider)
        {
            //If both are voxels and both belong to the same grid, don't check collision
            Voxel thisVoxel, otherVoxel;
            if (TryGetComponent<Voxel>(out thisVoxel) && (other as BoxCullider).TryGetComponent<Voxel>(out otherVoxel))
            {
                if (thisVoxel.grid == otherVoxel.grid)
                {
                    return CullisionInfo.NO_CULLISION;
                }
            }
            cullisionInfo = cullideWithBox(other as BoxCullider);
            if(cullisionInfo.cullided == false) return cullisionInfo;
            return addContactPoint(cullisionInfo);
        }
        if (other is SphereCullider)
        {
            cullisionInfo = (other as SphereCullider).cullideWithBox(this);
            cullisionInfo.first = this;
            cullisionInfo.second = other;
            cullisionInfo.normal *= -1;

            bool hasContactPointB=cullisionInfo.hasContactPointA;
            Vector3 contactPointB=cullisionInfo.contactPointA;

            cullisionInfo.hasContactPointA = cullisionInfo.hasContactPointB;
            cullisionInfo.contactPointA = cullisionInfo.contactPointB;
            cullisionInfo.hasContactPointB = hasContactPointB;
            cullisionInfo.contactPointB = contactPointB;
            //addContactPoint(cullisionInfo);
            return cullisionInfo;
        }
        return CullisionInfo.NO_CULLISION;
    }
    private CullisionInfo addContactPoint(CullisionInfo ci)
    {
        /*Voxel voxel;
        if (TryGetComponent<Voxel>(out voxel))
        {
            ci.hasContactPointA = true;
            ci.contactPointA = center;
        }
        if (ci.second is BoxCullider && (ci.second as BoxCullider).TryGetComponent<Voxel>(out voxel))
        {
            ci.hasContactPointB = true;
            ci.contactPointB = (ci.second as BoxCullider).center;
        }*/
        ci.hasContactPointA=true;
        ci.contactPointA=getDeepestVertex(ci.first as BoxCullider,ci.second as BoxCullider);
        ci.hasContactPointB=true;
        ci.contactPointB=getDeepestVertex(ci.second as BoxCullider,ci.first as BoxCullider);
        return ci;
    }

    private Vector3 getDeepestVertex(BoxCullider from,BoxCullider inside){
        Vector3[] fromVertices=from.vertices;
        Vector3 insideCenter=inside.center;
        Vector3 deepestVertex=fromVertices[0];
        float depth=float.MaxValue;

        foreach(Vector3 vertex in fromVertices){
            float distance=Vector3.Distance(vertex,insideCenter);
            if(distance<depth){
                depth=distance;
                deepestVertex=vertex;
            }
        }

        return deepestVertex;
    }
    private CullisionInfo cullideWithBox(BoxCullider other)
    {
        float overlap = float.MaxValue;
        //From other to base
        Vector3 axis = Vector3.zero;
        float tempOverlap;

        if ((tempOverlap = calculateOverlap(other, right)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = right;
        }
        if ((tempOverlap = calculateOverlap(other, up)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = up;
        }
        if ((tempOverlap = calculateOverlap(other, forward)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = forward;
        }
        if ((tempOverlap = calculateOverlap(other, other.right)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.right;
        }
        if ((tempOverlap = calculateOverlap(other, other.up)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.up;
        }
        if ((tempOverlap = calculateOverlap(other, other.forward)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.forward;
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(right, other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.right);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(right, other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.up);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(right, other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.forward);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(up, other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up, other.right);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(up, other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up, other.up);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(up, other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up, other.forward);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(forward, other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.right);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(forward, other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.up);
        }
        if ((tempOverlap = calculateOverlap(other, Vector3.Cross(forward, other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.forward);
        }
        Vector3 BinALocal = transform.InverseTransformPoint(other.center);

        Vector3 closestB = new Vector3(
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.x, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.y, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.z, 0.5f))
        );

        Vector3 AinBLocal = other.transform.InverseTransformPoint(center);

        Vector3 closestA = new Vector3(
            Mathf.Max(-0.5f, Mathf.Min(AinBLocal.x, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(AinBLocal.y, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(AinBLocal.z, 0.5f))
        );

        //Side Note: May cause problems if one box's center got inside the other, report if happened
        return new CullisionInfo(true, fixAxis(other, overlap, axis), overlap, false, false,
                                 transform.TransformPoint(closestB) * 0, other.transform.TransformPoint(closestA) * 0, this, other);
    }

    private float calculateOverlap(BoxCullider other, Vector3 axis)
    {
        // Handles the cross product = {0,0,0} case
        // Infinite cuz we'll take the smallest overlap 
        float axisMagnitude = axis.magnitude;
        if (axisMagnitude <= axisThreshold)
            return float.MaxValue;

        var aMin = float.MaxValue;
        var aMax = float.MinValue;
        var bMin = float.MaxValue;
        var bMax = float.MinValue;

        // Define two intervals, a and b. Calculate their min and max values
        for (var i = 0; i < 8; i++)
        {
            var aDist = Vector3.Dot(vertices[i], axis);
            aMin = aDist < aMin ? aDist : aMin;
            aMax = aDist > aMax ? aDist : aMax;
            var bDist = Vector3.Dot(other.vertices[i], axis);
            bMin = bDist < bMin ? bDist : bMin;
            bMax = bDist > bMax ? bDist : bMax;
        }

        // One-dimensional intersection test between a and b
        var longSpan = Mathf.Max(aMax, bMax) - Mathf.Min(aMin, bMin);
        var sumSpan = aMax - aMin + bMax - bMin;
        if (longSpan >= sumSpan)
        {
            return -1.0f;
        }
        else
        {
            return sumSpan - longSpan;
        }
    }

    static public Vector3 clampToClosestFace(Vector3 local)
    {
        float disToX = Mathf.Abs(0.5f - local.x);
        float disToMX = Mathf.Abs(-0.5f - local.x);
        float disToY = Mathf.Abs(0.5f - local.y);
        float disToMY = Mathf.Abs(-0.5f - local.y);
        float disToZ = Mathf.Abs(0.5f - local.z);
        float disToMZ = Mathf.Abs(-0.5f - local.z);

        float min = Mathf.Min(disToX, disToMX, disToY, disToMY, disToZ, disToMZ);

        if (min == disToX) return new Vector3(0.5f, local.y, local.z);
        if (min == disToMX) return new Vector3(-0.5f, local.y, local.z);
        if (min == disToY) return new Vector3(local.x, 0.5f, local.z);
        if (min == disToMY) return new Vector3(local.x, -0.5f, local.z);
        if (min == disToZ) return new Vector3(local.x, local.y, 0.5f);
        if (min == disToMZ) return new Vector3(local.x, local.y, -0.5f);
        return new Vector3(0.5f, local.y, local.z);
    }

    public void updateBoundaries()
    {

        center = transform.position;
        size = transform.localScale;
        rotation = transform.rotation;

        var max = size / 2;
        var min = -max;

        vertices = new[]
            {
                center + rotation * min,
                center + rotation * new Vector3(max.x, min.y, min.z),
                center + rotation * new Vector3(min.x, max.y, min.z),
                center + rotation * new Vector3(max.x, max.y, min.z),
                center + rotation * new Vector3(min.x, min.y, max.z),
                center + rotation * new Vector3(max.x, min.y, max.z),
                center + rotation * new Vector3(min.x, max.y, max.z),
                center + rotation * max,
           };

        right = rotation * Vector3.right;
        up = rotation * Vector3.up;
        forward = rotation * Vector3.forward;
    }
    private Vector3 fixAxis(BoxCullider other, float depth, Vector3 axis)
    {
        Vector3 otherToSelf = center - other.center;
        if (Vector3.Dot(axis, otherToSelf) >= 0)
            return axis;
        return -axis;
    }
    public bool isVoxel()
    {
        Voxel voxel;
        return TryGetComponent<Voxel>(out voxel);
    }

    public override string ToString()
    {
        Voxel v;
        if (TryGetComponent<Voxel>(out v))
        {
            return v.ToString();
        }
        return base.ToString();
    }

    public float3x3 getTensorInertia()
    {
        float3x3 tensor = float3x3.identity;
        float mass = rb.mass;
        float h = transform.localScale.y;
        float d = transform.localScale.z;
        float w = transform.localScale.x;

        mass = mass / 12.0f;

        tensor[0][0] = mass * (h * h + d * d);
        tensor[1][1] = mass * (w * w + d * d);
        tensor[2][2] = mass * (w * w + h * h);


        //Rotate tensor (in other words, take rotation into account when calculating inertia tensor)

        float3x3 rotationMat = (new float3x3(Matrix4x4.Rotate(rotation)));

        tensor = math.mul(math.mul(rotationMat, tensor), math.transpose(rotationMat));

        return tensor;
    }

    Vector3 Shape.center()
    {
        return center;
    }

    public RigidbodyDriver getRigidbodyDriver()
    {
        return GetComponent<RigidbodyDriver>();
    }
}