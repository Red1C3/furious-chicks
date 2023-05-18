using UnityEngine;

public abstract class Solver:MonoBehaviour{
    public abstract void resolveCullision(CullisionInfo[] cullisions);
}