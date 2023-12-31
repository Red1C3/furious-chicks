using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoxCullider : MonoBehaviour, Cullider
{
    [SerializeField]
    protected float frictionCo = 0.0f;
    [SerializeField]
    protected float bouncinessCo = 0.0f;
    public enum Side { TOP, DOWN, LEFT, RIGHT, FORWARD, BACKWARD, LEN }
    private Vector3 boxCenter;
    private Vector3 size;
    private Quaternion rotation;

    private Vector3[] vertices;
    protected Edge[] edges;
    private Vector3 right, up, forward;
    public Matrix4x4[] facesMats { get; protected set; }

    public static readonly float axisThreshold = 0.01f;
    private RigidbodyDriver rigidbodyDriver;
    private HashSet<Cullider> frameCulliders, stayedCulliders;
    private float3x3 localInertiaTensor;
    private float3x3 tempRotMat;
    private static List<Vector3> contactPoints=new List<Vector3>(8);
    private static List<Tuple<Edge,Edge>> contactEdges=new List<Tuple<Edge, Edge>>(8);

    private static bool[] thisEdgeValid = new bool[12];
    private static bool[] otherEdgeValid = new bool[12];

    private static float[] closestDistances = new float[6];
    protected virtual void Start()
    {
        frameCulliders = new HashSet<Cullider>();
        stayedCulliders = new HashSet<Cullider>();
        rigidbodyDriver = GetComponent<RigidbodyDriver>();
        facesMats = new Matrix4x4[6];
        edges = new Edge[12];
        localInertiaTensor = calculateLocalInertiaTensor();
        updateBoundaries();
    }

    protected virtual void FixedUpdate()
    {
        updateBoundaries();
    }
    public virtual Bounds getBounds()
    {
        //Requires mesh renderer with box mesh, otherwise use getBoxBounds
        return GetComponent<Renderer>().bounds;
    }

    public virtual Bounds getBoxBounds()
    {
        float minX, maxX, minY, maxY, minZ, maxZ;
        minX = minY = minZ = float.MaxValue;
        maxX = maxY = maxZ = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].x < minX) minX = vertices[i].x;
            if (vertices[i].x > maxX) maxX = vertices[i].x;
            if (vertices[i].y < minY) minY = vertices[i].y;
            if (vertices[i].y > maxY) maxY = vertices[i].y;
            if (vertices[i].z < minZ) minZ = vertices[i].z;
            if (vertices[i].z > maxZ) maxZ = vertices[i].z;
        }

        Bounds bounds = new Bounds();
        bounds.center = new Vector3((minX + maxX) / 2.0f, (minY + maxY) / 2.0f, (minZ + maxZ) / 2.0f);
        bounds.extents = (new Vector3(maxX, maxY, maxZ)) - bounds.center;
        return bounds;
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
            return cullisionInfo;
        }
        if (other is SphereCullider)
        {
            cullisionInfo = (other as SphereCullider).cullideWithBox(this);
            if (!cullisionInfo.cullided) return CullisionInfo.NO_CULLISION;
            cullisionInfo.first = this;
            cullisionInfo.second = other;
            cullisionInfo.normal *= -1;

            bool hasContactPointB = cullisionInfo.hasContactPointA;
            Vector3[] contactPointsB = (Vector3[])cullisionInfo.contactPointsA.Clone();

            cullisionInfo.hasContactPointA = cullisionInfo.hasContactPointB;
            cullisionInfo.contactPointsA = cullisionInfo.contactPointsB;
            cullisionInfo.hasContactPointB = hasContactPointB;
            cullisionInfo.contactPointsB = contactPointsB;
            return cullisionInfo;
        }
        return CullisionInfo.NO_CULLISION;
    }

    private CullisionInfo cullideWithBox(BoxCullider other)
    {
        Vector3 axis = Vector3.zero;
        bool thisOwnsReferenceFace = true;
        bool isEdgeContact = false;
        float overlap = float.MaxValue;
        float faceOverlap = float.MaxValue, edgeOverlap = float.MaxValue;
        Side side = Side.LEN;
        float tempOverlap;
        contactPoints.Clear();


        for (int i = 0; i < (int)Side.LEN; i++)
        {
            if ((tempOverlap = calculateOverlap(other, faceNormal((Side)i))) < 0)
            {
                return CullisionInfo.NO_CULLISION;
            }
            else if (isFaceValid(other, facesMats[i]) && tempOverlap < overlap)
            {
                overlap = tempOverlap;
                side = (Side)i;
                faceOverlap = overlap;
            }
        }

        for (int i = 0; i < (int)Side.LEN; i++)
        {
            if ((tempOverlap = calculateOverlap(other, other.faceNormal((Side)i))) < 0)
            {
                return CullisionInfo.NO_CULLISION;
            }
            else if (isFaceValid(other, other.facesMats[i]) && tempOverlap < overlap)
            {
                overlap = tempOverlap;
                side = (Side)i;
                thisOwnsReferenceFace = false;
                faceOverlap = overlap;
            }
        }

        contactEdges.Clear();
        
        for (int i = 0; i < edges.Length; i++)
        {
            thisEdgeValid[i] = isEdgeValid(other, edges[i]);
        }
        for (int i = 0; i < other.edges.Length; i++)
        {
            otherEdgeValid[i] = isEdgeValid(this, other.edges[i]);
        }


        for (int i = 0; i < edges.Length; i++)
        {
            for (int j = 0; j < other.edges.Length; j++)
            {
                if ((tempOverlap = calculateOverlap(other, Vector3.Cross(edges[i].vec(), other.edges[j].vec()).normalized)) < 0)
                {
                    return CullisionInfo.NO_CULLISION;
                }
                else if (isEdgeContact && thisEdgeValid[i] && otherEdgeValid[j] && (math.abs(overlap - tempOverlap)) < math.EPSILON)
                {
                    contactEdges.Add(new Tuple<Edge, Edge>(edges[i], other.edges[j]));
                }
                else if (thisEdgeValid[i] && otherEdgeValid[j])
                {
                    if (tempOverlap < overlap)
                    {
                        contactEdges.Clear();
                        isEdgeContact = true;
                        overlap = tempOverlap;
                        edgeOverlap = overlap;
                        contactEdges.Add(new Tuple<Edge, Edge>(edges[i], other.edges[j]));
                    }
                }
            }
        }

        if (!isEdgeContact && side != Side.LEN)
        {
            Matrix4x4 referenceFace;
            Face incidentFace;
            if (thisOwnsReferenceFace)
            {
                referenceFace = facesMats[(int)side];
                incidentFace = other.getIncidentFace(Face.normal(referenceFace));
            }
            else
            {
                referenceFace = other.facesMats[(int)side];
                incidentFace = getIncidentFace(Face.normal(referenceFace));
            }
            incidentFace.flip();

            Vector3[] incidentFacePoints = incidentFace.clip(referenceFace);

            contactPoints.AddRange(incidentFacePoints);
            axis = Face.normal(referenceFace);
            overlap = faceOverlap;
        }
        else if (isEdgeContact)
        {
            foreach (Tuple<Edge, Edge> tuple in contactEdges)
            {
                Vector3 contactPointA = tuple.Item1.closestPoint(tuple.Item2);
                Vector3 contactPointB = tuple.Item2.closestPoint(tuple.Item1);

                contactPoints.Add((contactPointA + contactPointB) / 2.0f);
            }
            axis = Vector3.Cross(contactEdges[0].Item1.vec(), contactEdges[0].Item2.vec()).normalized;
            overlap = edgeOverlap;
        }
        foreach (Vector3 contactPoint in contactPoints)
        {
            if (math.any(math.isnan(contactPoint)))
            {
                contactPoints.Clear();
                break;
            }
        }
        if (contactPoints.Count == 0)
        {
            contactPoints.Add((boxCenter + other.boxCenter) / 2.0f);
        }
        return new CullisionInfo(true, fixAxis(other, overlap, axis), overlap, true, true,
                                contactPoints.ToArray(), contactPoints.ToArray(), this, other);

    }
    private bool isFaceValid(BoxCullider other, Matrix4x4 faceMat)
    {
        Vector3 faceCenter = faceMat.GetColumn(3);
        float dot = Vector3.Dot(boxCenter - faceCenter, other.boxCenter - faceCenter);
        return dot < 0;
    }
    private bool isEdgeValid(BoxCullider box, Edge edge)
    {
        for (int i = 0; i < (int)Side.LEN; i++)
        {
            Vector3 faceNormal = box.faceNormal((Side)i);
            Vector3 faceCenter = box.facesMats[i] * (new Vector4(0, 0, 0, 1));

            Vector3 contactPoint;
            if (Plane.edgePlaneIntersection(out contactPoint, edge, faceNormal, faceCenter))
            {
                contactPoint = box.facesMats[i].inverse * (new Vector4(contactPoint.x, contactPoint.y, contactPoint.z, 1));
                if (contactPoint.x >= -1 && contactPoint.x <= 1 && contactPoint.z >= -1 && contactPoint.z <= 1) //Error this
                {
                    return true;
                }
            }
        }
        return false;
    }
    private Face getIncidentFace(Vector3 normal)
    {
        normal = normal.normalized;
        float smallestDot = float.MaxValue;
        Matrix4x4 incidentFaceMat = Matrix4x4.identity;

        for (int i = 0; i < (int)Side.LEN; i++)
        {
            Vector3 faceNorm = faceNormal((Side)i);
            float dot = Vector3.Dot(normal, faceNorm);
            if (dot < smallestDot)
            {
                smallestDot = dot;
                incidentFaceMat = facesMats[i];
            }
        }
        return new Face(incidentFaceMat);
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
        float disToX = closestDistances[0] = Vector3.Distance(local, new Vector3(0.5f, 0, 0));
        float disToMX = closestDistances[1] = Vector3.Distance(local, new Vector3(-0.5f, 0, 0));
        float disToY = closestDistances[2] = Vector3.Distance(local, new Vector3(0, 0.5f, 0));
        float disToMY = closestDistances[3] = Vector3.Distance(local, new Vector3(0, -0.5f, 0));
        float disToZ = closestDistances[4] = Vector3.Distance(local, new Vector3(0, 0, 0.5f));
        float disToMZ = closestDistances[5] = Vector3.Distance(local, new Vector3(0, 0, -0.5f));

        Array.Sort(closestDistances);
        for (int i = 0; i < closestDistances.Length; i++)
        {
            Vector3 clampedPoint;
            if (closestDistances[i] == disToX) clampedPoint = new Vector3(0.5f, local.y, local.z);
            else if (closestDistances[i] == disToMX) clampedPoint = new Vector3(-0.5f, local.y, local.z);
            else if (closestDistances[i] == disToY) clampedPoint = new Vector3(local.x, 0.5f, local.z);
            else if (closestDistances[i] == disToMY) clampedPoint = new Vector3(local.x, -0.5f, local.z);
            else if (closestDistances[i] == disToZ) clampedPoint = new Vector3(local.x, local.y, 0.5f);
            else clampedPoint = new Vector3(local.x, local.y, -0.5f);


            if (clampedPoint.x <= 0.5f + 0.01f && clampedPoint.x >= -0.5f - 0.01f &&
            clampedPoint.y <= 0.5f + 0.01f && clampedPoint.y >= -0.5f - 0.01f &&
            clampedPoint.z <= 0.5f + 0.01f && clampedPoint.z >= -0.5f - 0.01f)
                return clampedPoint;
        }
        return local;
    }

    public void updateBoundaries()
    {

        boxCenter = transform.position;
        size = transform.lossyScale;
        rotation = transform.rotation;

        var max = size / 2;
        var min = -max;

        vertices = new[]
            {
                boxCenter + rotation * min,                             //0
                boxCenter + rotation * new Vector3(max.x, min.y, min.z),//1
                boxCenter + rotation * new Vector3(min.x, max.y, min.z),//2
                boxCenter + rotation * new Vector3(max.x, max.y, min.z),//3
                boxCenter + rotation * new Vector3(min.x, min.y, max.z),//4
                boxCenter + rotation * new Vector3(max.x, min.y, max.z),//5
                boxCenter + rotation * new Vector3(min.x, max.y, max.z),//6
                boxCenter + rotation * max,                             //7
           };

        right = rotation * Vector3.right;
        up = rotation * Vector3.up;
        forward = rotation * Vector3.forward;

        //Bottom face
        edges[0] = new Edge(vertices[0], vertices[4]);
        edges[1] = new Edge(vertices[4], vertices[5]);
        edges[2] = new Edge(vertices[5], vertices[1]);
        edges[3] = new Edge(vertices[1], vertices[0]);

        //Top face
        edges[4] = new Edge(vertices[7], vertices[3]);
        edges[5] = new Edge(vertices[3], vertices[2]);
        edges[6] = new Edge(vertices[2], vertices[6]);
        edges[7] = new Edge(vertices[6], vertices[7]);

        //Side faces
        edges[8] = new Edge(vertices[7], vertices[5]);
        edges[9] = new Edge(vertices[0], vertices[2]);
        edges[10] = new Edge(vertices[6], vertices[4]);
        edges[11] = new Edge(vertices[1], vertices[3]);


        //Top
        facesMats[(int)Side.TOP] = math.mul(float4x4.Translate(boxCenter + up * max.y), new float4x4(new float4(right * max.x, 0),
                                                                                new float4(up * max.y, 0),
                                                                                new float4(forward * max.z, 0),
                                                                                new float4(0, 0, 0, 1)));

        //Bottom
        facesMats[(int)Side.DOWN] = math.mul(float4x4.Translate(boxCenter - up * max.y), new float4x4(new float4(right * max.x, 0),
                                                                                new float4(-up * max.y, 0),
                                                                                new float4(-forward * max.z, 0),
                                                                                new float4(0, 0, 0, 1)));

        //Right
        facesMats[(int)Side.RIGHT] = math.mul(float4x4.Translate(boxCenter + right * max.x), new float4x4(new float4(-up * max.y, 0),
                                                                        new float4(right * max.x, 0),
                                                                        new float4(forward * max.z, 0),
                                                                        new float4(0, 0, 0, 1)));

        //Left
        facesMats[(int)Side.LEFT] = math.mul(float4x4.Translate(boxCenter - right * max.x), new float4x4(new float4(up * max.y, 0),
                                                                        new float4(-right * max.x, 0),
                                                                        new float4(forward * max.z, 0),
                                                                        new float4(0, 0, 0, 1)));

        //Forward
        facesMats[(int)Side.FORWARD] = math.mul(float4x4.Translate(boxCenter + forward * max.z), new float4x4(new float4(right * max.x, 0),
                                                                        new float4(forward * max.z, 0),
                                                                        new float4(-up * max.y, 0),
                                                                        new float4(0, 0, 0, 1)));

        //Backward
        facesMats[(int)Side.BACKWARD] = math.mul(float4x4.Translate(boxCenter - forward * max.z), new float4x4(new float4(right * max.x, 0),
                                                                        new float4(-forward * max.z, 0),
                                                                        new float4(up * max.y, 0),
                                                                        new float4(0, 0, 0, 1)));
    }
    private Vector3 faceNormal(Side face)
    {
        Vector3 norm = facesMats[(int)face] * Vector3.up;
        return norm.normalized;
    }
    private Vector3 fixAxis(BoxCullider other, float depth, Vector3 axis)
    {
        Vector3 otherToSelf = boxCenter - other.boxCenter;
        if (Vector3.Dot(axis, otherToSelf) >= 0)
            return axis;
        return -axis;
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

    private float3x3 calculateLocalInertiaTensor()
    {
        float3x3 tensor = float3x3.identity;
        float mass = rigidbodyDriver.mass;
        float h = transform.localScale.y; 
        float d = transform.localScale.z;
        float w = transform.localScale.x;

        mass = mass / 12.0f;

        tensor[0][0] = mass * (h * h + d * d);
        tensor[1][1] = mass * (w * w + d * d);
        tensor[2][2] = mass * (w * w + h * h);

        return tensor;
    }
    public virtual float3x3 getTensorInertia()
    {
        if (rotation == Quaternion.identity) return localInertiaTensor;

        //Rotate tensor (in other words, take rotation into account when calculating inertia tensor)

        Matrix4x4 rotMat = Matrix4x4.Rotate(rotation);

        tempRotMat.c0 = (Vector3)rotMat.GetColumn(0);
        tempRotMat.c1 = (Vector3)rotMat.GetColumn(1);
        tempRotMat.c2 = (Vector3)rotMat.GetColumn(2);

        return math.mul(math.mul(tempRotMat, localInertiaTensor), math.transpose(tempRotMat));
    }

    public virtual Vector3 center()
    {
        return boxCenter;
    }

    public virtual RigidbodyDriver getRigidbodyDriver()
    {
        return rigidbodyDriver;
    }

    public float getFrictionCo()
    {
        return frictionCo;
    }
    public float getBouncinessCo()
    {
        return bouncinessCo;
    }
    public virtual HashSet<Cullider> getFrameCulliders()
    {
        return frameCulliders;
    }
    public virtual HashSet<Cullider> getStayedCulliders()
    {
        return stayedCulliders;
    }
    public void updateLocalInertiaTensor(){
        localInertiaTensor=calculateLocalInertiaTensor();
    }
    public void setFrictionCo(float val){
        frictionCo=val;
    }
    public void setBouncinessCo(float val){
        bouncinessCo=val;
    }
}