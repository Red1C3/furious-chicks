using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject player;
    public Solver solver;
    Octree octree;
    public static int nodeMinSize = 1;
    public static int allObjectsN = 0, maxNodeObjectN = 0;

    private List<GameObject> cullidingObject;
    public static List<CullisionInfo> culls = new List<CullisionInfo>();

    // Start is called before the first frame update
    void Start()
    {
        cullidingObject = new List<GameObject>();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.tag == "Player")
                continue;
            Cullider cullider;
            if (gameObject.TryGetComponent<Cullider>(out cullider))
            {
                cullidingObject.Add(gameObject);
            }
        }

        octree = new Octree(cullidingObject, nodeMinSize);
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
        foreach (GameObject go in cullidingObject)
        {
            octree.search(go, solver);
        }
        octree.Update(cullidingObject, nodeMinSize);

        foreach (CullisionInfo cullision in culls)
        {
            solver.resolveCullision(cullision, cullision.first.getRigidbody(), cullision.second.getRigidbody());
        }
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            rb.physicsUpdate();
        }

        int cullidingObjectN = cullidingObject.Count;
        if(2 * cullidingObjectN < allObjectsN){
            if(nodeMinSize > 10)
                nodeMinSize += (int) Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize += 1;
        }
        if(2 * cullidingObjectN < nodeMinSize * maxNodeObjectN){
            if(nodeMinSize > 10)
                nodeMinSize -= (int) Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize -= 1;
        }
    }
}
