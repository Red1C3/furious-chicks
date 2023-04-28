using UnityEngine;

public class OffsetSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        float AmassFactor = A.mass / (A.mass + B.mass);
        float BmassFactor = B.mass / (A.mass + B.mass);
        A.transform.position += BmassFactor * cullision.normal.normalized * cullision.depth;
        B.transform.position += -AmassFactor * cullision.normal.normalized * cullision.depth;

        A.GetComponent<RigidbodyDriver>().applyLinearMomentum(B.GetComponent<RigidbodyDriver>());
    }
}