using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject player;
    public int nodeMinSize = 5;
    public Solver solver;
    Octree octree;

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

        solver.resolveCullision(culls.ToArray());
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            rb.physicsUpdate();
        }
    }
}
