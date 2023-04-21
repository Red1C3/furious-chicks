using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject player;
    public int nodeMinSize = 5;  
    public SimpleSolver solver;
    Octree octree;

    private List<GameObject> cullidingObject;

    // Start is called before the first frame update
    void Start()
    {
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
        octree.search(player,solver);
        foreach(GameObject go in cullidingObject){
            octree.search(go,solver);
        }
        octree.Update(cullidingObject,nodeMinSize);
    }
}
