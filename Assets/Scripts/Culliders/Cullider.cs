using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Cullider : Shape
{
    CullisionInfo cullideWith(Cullider other);
    Bounds getBounds();
    RigidbodyDriver getRigidbodyDriver();
    float getFrictionCo();
    float getBouncinessCo();
    HashSet<Cullider> getFrameCulliders(); //Must initialize at start
    HashSet<Cullider> getStayedCulliders();
    static List<Cullider> clonedStayed;
    void triggerCulliders()
    {
        HashSet<Cullider> frameCulliders = getFrameCulliders();
        HashSet<Cullider> stayedCulliders = getStayedCulliders();

        foreach (Cullider cullider in frameCulliders)
        {
            if (stayedCulliders.Contains(cullider))
            {
                getRigidbodyDriver().onCullisionStay(cullider);
            }
            else
            {
                getRigidbodyDriver().onCullisionEnter(cullider);
                stayedCulliders.Add(cullider);
            }
        }

        clonedStayed.Clear();
        clonedStayed.AddRange(stayedCulliders);
        foreach (Cullider cullider in clonedStayed)
        {
            if (!frameCulliders.Contains(cullider))
            {
                getRigidbodyDriver().onCullisionExit(cullider);
                stayedCulliders.Remove(cullider);
            }
        }
    }
}

public struct CullisionInfo
{
    public bool cullided;
    public Vector3 normal, t1, t2;
    public float depth;
    public bool hasContactPointA, hasContactPointB;
    public Vector3[] contactPointsA, contactPointsB;

    public Cullider first, second;
    public float normalImpulseSum, tangentImpulseSum1, tangentImpulseSum2;

    public static readonly CullisionInfo NO_CULLISION = new CullisionInfo(false, Vector3.zero, 0, false, false, new Vector3[] { }, new Vector3[] { }, null, null);

    public CullisionInfo(bool cullided, Vector3 normal, float depth, bool hasContactPointA,
                   bool hasContactPointB, Vector3 contactPointA, Vector3 contactPointB,
                   Cullider first, Cullider second) : this(cullided, normal, depth, hasContactPointA, hasContactPointB,
                   new[] { contactPointA }, new[] { contactPointB }, first, second)
    { }

    public CullisionInfo(bool cullided, Vector3 normal, float depth, bool hasContactPointA,
                    bool hasContactPointB, Vector3[] contactPointsA, Vector3[] contactPointsB,
                    Cullider first, Cullider second)
    {
        bool debug = false;

        normalImpulseSum = 0.0f;
        tangentImpulseSum1 = 0.0f;
        tangentImpulseSum2 = 0.0f;

        this.cullided = cullided;
        this.normal = -normal.normalized;
        this.depth = depth;
        this.contactPointsA = contactPointsA;
        this.contactPointsB = contactPointsB;
        this.hasContactPointA = hasContactPointA;
        this.hasContactPointB = hasContactPointB;
        this.first = first;
        this.second = second;

        if (this.normal.x >= 0.57735f)
        {
            t1 = new Vector3(this.normal.y, -this.normal.x, 0.0f);
        }
        else
        {
            t1 = new Vector3(0.0f, this.normal.z, -this.normal.y);
        }

        t1 = t1.normalized;
        t2 = Vector3.Cross(this.normal, t1);

        if (debug)
        {
            if (cullided)
            {
                Debug.Log(ToString());
            }
        }
        if (cullided)
        {
            first.getFrameCulliders().Add(second);
            second.getFrameCulliders().Add(first);
        }
    }

    public override string ToString()
    {
        string str = "Reporting collision\n" +
                "Colliding objects:" + first.ToString() + "," + second.ToString() + "\n" +
                "depth=" + depth + "\n" +
                "normal=" + normal.x + "," + normal.y + "," + normal.z + "\n" +
                "has contact point for first:" + hasContactPointA + "\n" +
                "has contact point for second:" + hasContactPointB + "\n";

        for (int i = 0; i < contactPointsA.Length; i++)
        {
            str += "Contact Point A No" + i + ":" + contactPointsA[i] + "\n";
        }
        for (int i = 0; i < contactPointsB.Length; i++)
        {
            str += "Contact Point B No" + i + ":" + contactPointsB[i] + "\n";
        }
        return str;
    }
}