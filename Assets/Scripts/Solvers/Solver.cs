using UnityEngine;

public abstract class Solver:MonoBehaviour{
    public abstract void resolveCullision(CullisionInfo cullision,Rigidbody A,Rigidbody B);
}