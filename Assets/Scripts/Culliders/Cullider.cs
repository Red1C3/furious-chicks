using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Cullider
{
    CullisionInfo cullideWith(Cullider other);
}

public struct CullisionInfo
{
    public bool cullided;
    public Vector3 normal;
    public float depth;
    public bool hasContactPointA, hasContactPointB;
    public Vector3 contactPointA, contactPointB;


    public static readonly CullisionInfo NO_CULLISION=new CullisionInfo(false,Vector3.zero,0,false,false,Vector3.zero,Vector3.zero);
    
    public CullisionInfo(bool cullided, Vector3 normal, float depth, bool hasContactPointA,
                    bool hasContactPointB, Vector3 contactPointA, Vector3 contactPointB)
    {
        this.cullided = cullided;
        this.normal = normal;
        this.depth = depth;
        this.contactPointA = contactPointA;
        this.contactPointB = contactPointB;
        this.hasContactPointA = hasContactPointA;
        this.hasContactPointB = hasContactPointB;
    }
}