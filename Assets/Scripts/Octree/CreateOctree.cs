using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject[] world;
    public GameObject player;
    public int nodeMinSize = 5;  
    public SimpleSolver solver;
    Octree octree;
    // Start is called before the first frame update
    void Start()
    {
        octree = new Octree(world,nodeMinSize);
    }

    void OnDrawGizmos(){
        if(Application.isPlaying){
            octree.rootNode.Draw();
        }
    }

    void FixedUpdate(){
        octree.search(player,solver);
        octree.Update(world,nodeMinSize);
    }
}
