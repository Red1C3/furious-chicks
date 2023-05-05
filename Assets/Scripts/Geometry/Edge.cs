using UnityEngine;

public struct Edge{
    public Vector3 from{get; private set;}
    public Vector3 to{get; private set;}
    public Edge(Vector3 from,Vector3 to){
        this.from=from;
        this.to=to;
    }
}