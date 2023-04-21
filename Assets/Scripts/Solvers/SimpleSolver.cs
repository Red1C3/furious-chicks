using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSolver : MonoBehaviour
{
    public void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if (!cullision.cullided) return;

        cullision.depth*=10;

        if (cullision.hasContactPointA)
        {
            A.AddForceAtPosition(cullision.normal.normalized * cullision.depth, cullision.contactPointA, ForceMode.Impulse);
        }
        else
        {
            A.AddForce(cullision.normal.normalized * cullision.depth, ForceMode.Impulse);
        }

        if (cullision.hasContactPointB)
        {
            B.AddForceAtPosition(-cullision.normal.normalized * cullision.depth, cullision.contactPointB, ForceMode.Impulse);
        }
        else
        {
            B.AddForce(-cullision.normal.normalized * cullision.depth, ForceMode.Impulse);
        }
    }
}
