using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public Solver solver;
    GameObject player,Ground;
    Octree octree;
    public static int nodeMinSize = 0;
    public static int allObjectsN = 0, maxNodeObjectN = 0;

    bool lastActionIsShrink=false;

    private List<GameObject> cullidingObject;
    public static List<CullisionInfo> culls = new List<CullisionInfo>();

    // Start is called before the first frame update
    void Start()
    {
        cullidingObject = new List<GameObject>();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.tag == "Player"){
                player=gameObject;
                continue;
            }
            else if(gameObject.tag=="Ground"){
                Ground=gameObject;
                continue;
            }
            Cullider cullider;
            if (gameObject.TryGetComponent<Cullider>(out cullider))
            {
                cullidingObject.Add(gameObject);
            }
        }
        if(nodeMinSize==0){
            octree = new Octree(cullidingObject);
            Debug.Log("Node Size: "+nodeMinSize);
        }
        else
            octree = new Octree(cullidingObject,nodeMinSize);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            octree.rootNode.Draw();
        }
    }

    void FixedUpdate()
    {
        allObjectsN = 0;
        maxNodeObjectN = 0;
        culls.Clear();
        RigidbodyDriver[] rigidbodies = FindObjectsOfType<RigidbodyDriver>();
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            rb.applyForces();
        }

        octree.search(player, solver);
        octree.search(Ground, solver);

        foreach (GameObject go in cullidingObject)
        {
            octree.search(go, solver);
        }
        octree.Update(cullidingObject, nodeMinSize);

        solver.resolveCullision(culls.ToArray());
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            rb.physicsUpdate();
        }

        int cullidingObjectN = cullidingObject.Count;
        if((lastActionIsShrink && 2 * cullidingObjectN < allObjectsN) || (4 * cullidingObjectN < allObjectsN)){
            Debug.Log("Expand");
            if(nodeMinSize > 10)
                nodeMinSize += (int) Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize += 1;
        }
        if((!lastActionIsShrink && 2 * cullidingObjectN < nodeMinSize * maxNodeObjectN) || (2 * cullidingObjectN < nodeMinSize * maxNodeObjectN)){
            Debug.Log("Shrinking");
            if(nodeMinSize > 10)
                nodeMinSize -= (int) Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize -= 1;
        }
    }
}
