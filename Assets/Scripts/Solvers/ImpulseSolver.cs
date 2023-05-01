using UnityEngine;
using Unity.Mathematics;

public class ImpulseSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if(!cullision.cullided) return;
        Vector3 normal=cullision.normal.normalized*cullision.depth;

        Vector3 rA=cullision.contactPointA-cullision.first.center();
        Vector3 rB=cullision.contactPointB-cullision.second.center();

        float12 jacobian=new float12(-normal,Vector3.Cross(-normal,rA)
                                    ,normal,Vector3.Cross(normal,rB));

        
    }
}