using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Cullider
{
    Rigidbody getRigidbody();
    CullisionInfo cullideWith(Cullider other);
    Bounds getBounds();
}

public struct CullisionInfo
{
    public bool cullided;
    public Vector3 normal;
    public float depth;
    public bool hasContactPointA, hasContactPointB;
    public Vector3 contactPointA, contactPointB;

    private Cullider first,second;

    public static readonly CullisionInfo NO_CULLISION=new CullisionInfo(false,Vector3.zero,0,false,false,Vector3.zero,Vector3.zero,null,null);
    
    public CullisionInfo(bool cullided, Vector3 normal, float depth, bool hasContactPointA,
                    bool hasContactPointB, Vector3 contactPointA, Vector3 contactPointB,
                    Cullider first,Cullider second)
    {
        bool debug=true;

        this.cullided = cullided;
        this.normal = normal;
        this.depth = depth;
        this.contactPointA = contactPointA;
        this.contactPointB = contactPointB;
        this.hasContactPointA = hasContactPointA;
        this.hasContactPointB = hasContactPointB;
        this.first=first;
        this.second=second;

        if(debug){
            if(cullided){
                Debug.Log(ToString());
            }
        }
    }

    public override string ToString(){
        return "Reporting collision\n"+
                "Colliding objects:"+first.ToString()+","+second.ToString()+"\n"+
                "depth="+depth+"\n"+
                "normal="+normal.x+","+normal.y+","+normal.z+"\n"+
                "has contact point for first:"+hasContactPointA+"\n"+
                "has contact point for second:"+hasContactPointB+"\n"+
                "first contact point:"+contactPointA.x+","+contactPointA.y+","+contactPointA.z+"\n"+
                "second contact point:"+contactPointB.x+","+contactPointB.y+","+contactPointB.z;
    }
}