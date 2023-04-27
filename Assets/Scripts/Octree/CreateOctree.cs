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
        Time.timeScale=0.5f;
        cullidingObject = new List<GameObject>();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            if(gameObject.tag=="Player")
                continue;
            Cullider cullider;
            if (gameObject.TryGetComponent<Cullider>(out cullider))
            {
                cullidingObject.Add(gameObject);
            }
        }

        octree = new Octree(cullidingObject,nodeMinSize);
    }

    void OnDrawGizmos(){
        if(Application.isPlaying){
            octree.rootNode.Draw();
        }
    }

    void FixedUpdate(){
        culls.Clear();
        octree.search(player,solver);
        foreach(GameObject go in cullidingObject){
            octree.search(go,solver);
        }
        octree.Update(cullidingObject,nodeMinSize);
        foreach(CullisionInfo cullision in culls){
            solver.resolveCullision(cullision,cullision.first.getRigidbody(), cullision.second.getRigidbody());
            Debug.Log("Count: "+culls.Count);
        }
    }
}
