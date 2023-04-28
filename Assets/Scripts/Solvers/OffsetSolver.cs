using UnityEngine;

public class OffsetSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        B.transform.position+=-cullision.normal.normalized*cullision.depth;
    }
}