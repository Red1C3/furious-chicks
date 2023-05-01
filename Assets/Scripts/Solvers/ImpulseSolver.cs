using UnityEngine;

public class ImpulseSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if(!cullision.cullided) return;
    }
}