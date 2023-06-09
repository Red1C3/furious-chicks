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
            if (cullisionInfo.cullided == false) return cullisionInfo;
            //return addContactPoint(cullisionInfo);
            return cullisionInfo;
        }
        if (other is SphereCullider)
        {
            cullisionInfo = (other as SphereCullider).cullideWithBox(this);
            cullisionInfo.first = this;
            cullisionInfo.second = other;
            cullisionInfo.normal *= -1;

            bool hasContactPointB = cullisionInfo.hasContactPointA;
            Vector3[] contactPointsB = (Vector3[])cullisionInfo.contactPointsA.Clone();

            cullisionInfo.hasContactPointA = cullisionInfo.hasContactPointB;
            cullisionInfo.contactPointsA = cullisionInfo.contactPointsB;
            cullisionInfo.hasContactPointB = hasContactPointB;
            cullisionInfo.contactPointsB = contactPointsB;
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
        /*ci.hasContactPointA = true;
        ci.contactPointA = getDeepestVertex(ci.first as BoxCullider, ci.second as BoxCullider);
        ci.hasContactPointB = true;
        ci.contactPointB = getDeepestVertex(ci.second as BoxCullider, ci.first as BoxCullider);*/
        return ci;
    }

    private Vector3 getDeepestVertex(BoxCullider from, BoxCullider inside)
    {
        Vector3[] fromVertices = from.vertices;
        Vector3 insideCenter = inside.boxCenter;
        Vector3 deepestVertex = fromVertices[0];
        float depth = float.MaxValue;

        foreach (Vector3 vertex in fromVertices)
        {
            float distance = Vector3.Distance(vertex, insideCenter);
            if (distance < depth)
            {
                depth = distance;
                deepestVertex = vertex;
            }
        }

        return deepestVertex;
    }
    private CullisionInfo cullideWithBox(BoxCullider other)
    {
        Vector3 axis = Vector3.zero;
        bool thisOwnsReferenceFace = true;
        bool isEdgeContact = false;
        //Vector3 centeralContactPoint = Vector3.zero;
        float overlap = float.MaxValue;
        float faceOverlap = float.MaxValue, edgeOverlap = float.MaxValue;
        Side side = Side.LEN;
        float tempOverlap;
        List<Vector3> contactPoints = new List<Vector3>();


        //FIXME calculate overlap only takes the vector into account which returns the opposite faces
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

        List<Tuple<Edge, Edge>> contactEdges = new List<Tuple<Edge, Edge>>();
        bool[] thisEdgeValid = new bool[12];
        bool[] otherEdgeValid = new bool[12];
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
                        //thisEdge = edges[i];
                        //otherEdge = other.edges[j];
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
            //     if (isEdgeContact)
            //     {
            //         Vector3 edgeAxis = Vector3.Cross(contactEdges[0].Item1.vec(), contactEdges[0].Item2.vec());
            //         if (Vector3.Dot(fixAxis(other, faceOverlap, axis).normalized, fixAxis(other, edgeOverlap, edgeAxis).normalized) < 1.0f - math.EPSILON)
            //         {
            //            // Debug.Log("Edge contact 0");
            //             axis = edgeAxis;
            //             overlap = edgeOverlap;
            //             contactPoints.Clear();
            //             foreach (Tuple<Edge, Edge> tuple in contactEdges)
            //             {
            //                 Vector3 contactPointA = tuple.Item1.closestPoint(tuple.Item2);
            //                 Vector3 contactPointB = tuple.Item2.closestPoint(tuple.Item1);

            //                 contactPoints.Add((contactPointA + contactPointB) / 2.0f);
            //             }
            //         }

            //     }
        }
        else if (isEdgeContact)
        {
            // Debug.Log("Edge contact 1");
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
                    // Debug.Log(faceCenter);
                    // Debug.Log(faceNormal);
                    // Debug.Log(edge);

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
    private CullisionInfo cullideWithBoxOld(BoxCullider other)
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
        Vector3 BinALocal = transform.InverseTransformPoint(other.boxCenter);

        Vector3 closestB = new Vector3(
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.x, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.y, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(BinALocal.z, 0.5f))
        );

        Vector3 AinBLocal = other.transform.InverseTransformPoint(boxCenter);

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

    private float3x3 calculateLocalInertiaTensor()
    {
        float3x3 tensor = float3x3.identity;
        float mass = rigidbodyDriver.mass;
        float h = transform.localScale.y; //FIXME in case of voxels, multply by parents scale
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
        //Rotate tensor (in other words, take rotation into account when calculating inertia tensor)

        float3x3 rotationMat = new float3x3(rotation);
        
        return math.mul(math.mul(rotationMat, localInertiaTensor), math.transpose(rotationMat));
    }

    public virtual Vector3 center()
    {
        return boxCenter;
    }

    public virtual RigidbodyDriver getRigidbodyDriver()
    {
        return GetComponent<RigidbodyDriver>();
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
}