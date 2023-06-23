using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public Solver solver;
    GameObject player;
    public static GameObject ground { get; private set; }
    Octree octree;
    public static int nodeMinSize = 0;
    public static float maxSpeed = 0, threshhold = 0.02f;
    public static int allObjectsN = 0, maxNodeObjectN = 0;

    bool lastActionIsShrink = false;

    private List<GameObject> cullidingObject;
    public static List<CullisionInfo> culls = new List<CullisionInfo>();
    private List<GameObject> cullidingObjectsCopy;
    private Throw playerThrow;

    // Start is called before the first frame update
    void Start()
    {
        cullidingObject = new List<GameObject>();
        cullidingObjectsCopy = new List<GameObject>();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.tag == "Player")
                continue;
            else if (gameObject.tag == "Ground")
            {
                ground = gameObject;
                continue;
            }
            Cullider cullider;
            if (gameObject.TryGetComponent<Cullider>(out cullider))
            {
                cullidingObject.Add(gameObject);
            }
        }
        if (nodeMinSize == 0)
        {
            octree = new Octree(cullidingObject);
        }
        else
            octree = new Octree(cullidingObject, nodeMinSize);

        Cullider.clonedStayed = new List<Cullider>(cullidingObject.Count + 16);
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
        if (player.GetComponent<Cullider>().getFrameCulliders() == null) return;
        maxSpeed = 0;
        allObjectsN = 0;
        maxNodeObjectN = 0;
        culls.Clear();
        cullidingObjectsCopy.Clear();
        RigidbodyDriver[] rigidbodies = FindObjectsOfType<RigidbodyDriver>();
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            Cullider cullider;
            if (rb.TryGetComponent<Cullider>(out cullider))
            {
                cullider.getFrameCulliders().Clear();
            }
            rb.applyForces();
            maxSpeed = Mathf.Max(rb.velocity.magnitude, maxSpeed);
        }
        if (cullidingObject.Count > 0)
        {
            octree.Update(cullidingObject, nodeMinSize);
            if (playerThrow.fired)
                octree.search(player, solver);
            octree.search(ground, solver);
        }
        if (playerThrow.fired)
            octree.rootNode.checkCulliding(player.GetComponent<Cullider>(), ground.GetComponent<Cullider>());

        foreach (GameObject go in cullidingObject)
        {
            octree.search(go, solver);
        }

        solver.resolveCullision(culls.ToArray());
        foreach (RigidbodyDriver rb in rigidbodies)
        {
            rb.physicsUpdate();
        }
        cullidingObjectsCopy.AddRange(cullidingObject);
        foreach (GameObject obj in cullidingObjectsCopy)
        {
            Cullider cullider;
            if (obj.TryGetComponent<Cullider>(out cullider))
            {
                cullider.triggerCulliders();
            }
        }
        player.GetComponent<Cullider>().triggerCulliders();

        int cullidingObjectN = cullidingObject.Count;
        if ((lastActionIsShrink && 2 * cullidingObjectN < allObjectsN) || (4 * cullidingObjectN < allObjectsN))
        {
            if (nodeMinSize > 10)
                nodeMinSize += (int)Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize += 1;
        }
        if ((!lastActionIsShrink && 2 * cullidingObjectN < nodeMinSize * maxNodeObjectN) || (2 * cullidingObjectN < nodeMinSize * maxNodeObjectN))
        {
            if (nodeMinSize > 10)
                nodeMinSize -= (int)Mathf.Ceil(nodeMinSize / 2);
            else
                nodeMinSize -= 1;
        }
        if (maxSpeed > 0.0f && maxSpeed * threshhold > 2.0f)
            Time.fixedDeltaTime = 2.0f / maxSpeed;
        else
            Time.fixedDeltaTime = threshhold;
    }
    public BirdBase setPlayer(GameObject player)
    {
        player = Instantiate(player, Vector3.zero, Quaternion.identity);
        this.player = player;
        playerThrow = this.player.AddComponent<Throw>();
        playerThrow.lineRenderer = LevelCtrlr.line;
        playerThrow.lineRenderer.enabled = true;
        playerThrow.parabolaRenderer=LevelCtrlr.parabola;
        playerThrow.parabolaRenderer.enabled=true;
        return player.GetComponent<BirdBase>();
    }
    public void removeCullider(GameObject cullider)
    {
        cullidingObject.Remove(cullider);
    }
    public void addCullider(GameObject cullider)
    {
        cullidingObject.Add(cullider);
    }
    public int getCullidingObjectsNum()
    {
        return cullidingObject.Count;
    }
}
