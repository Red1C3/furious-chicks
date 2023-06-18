using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrlr : MonoBehaviour
{
    [SerializeField]
    private GameObject[] birds;
    private bool throwingPhase = true;
    [SerializeField]
    private GameObject linePrefab;
    private LineRenderer line;
    public Throw currentBirdThrow { get; private set; }
    public CreateOctree engine { get; private set; }
    private int pigsCount;
    private int destroyedPigs = 0;
    public static bool playerView { get; set; }
    public static bool once;
    public static Camera cam;
    Quaternion currentRotation;
    Vector3 currentEulerAngles;
    private BirdBase currentBird;
    [SerializeField]
    private bool randomBird = true;
    private int currentBirdIndex = 0;
    [SerializeField]
    private bool createlvlUI = false;
    [SerializeField]
    private GameObject lvlUiPrefab;
    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        engine = FindObjectOfType<CreateOctree>();
        pigsCount = FindObjectsOfType<PigBase>().Length;
        if (randomBird)
        {
            int rand = Random.Range(0, birds.Length);
            currentBird = engine.setPlayer(birds[rand]);
        }
        else
        {
            currentBird = engine.setPlayer(birds[currentBirdIndex]);
        }
        cam = FindObjectOfType<Camera>();
        playerView = true;
        once = true;

        if (createlvlUI)
        {
            Instantiate(lvlUiPrefab, FindObjectOfType<Canvas>().transform);
        }
    }


    private void Update()
    {
        if (throwingPhase)
        {
            currentBirdThrow = currentBird.gameObject.AddComponent<Throw>();
            currentBirdThrow.lineRenderer = line;
            currentBirdThrow.lineRenderer.enabled = true;
            throwingPhase = false;
        }
        else if (currentBirdThrow.hasFired())
        {
            currentBird.hasFired = true;
            if (currentBird.isDead())
            {
                Destroy(currentBird.gameObject);
                if (!randomBird && currentBirdIndex + 1 == birds.Length)
                {
                    gameOver();
                }
                else
                {
                    if (randomBird)
                    {
                        int rand = Random.Range(0, birds.Length);
                        currentBird = engine.setPlayer(birds[rand]);
                    }
                    else
                    {
                        currentBirdIndex++;
                        currentBird = engine.setPlayer(birds[currentBirdIndex]);
                    }
                    throwingPhase = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentBirdThrow.fired)
                playerView = false;
            else
                playerView = !playerView;
            once = true;
        }

        if (playerView && once)
        {
            currentRotation.eulerAngles = Vector3.zero;
            cam.transform.rotation = currentRotation;

            cam.transform.position = new Vector3(currentBird.transform.position.x, currentBird.transform.position.y, -(currentBirdThrow.force + currentBirdThrow.cameraAway));
            once = false;
        }
        else if (once)
        {
            currentRotation.eulerAngles = new Vector3(0, 90, 0);
            cam.transform.rotation = currentRotation;
            Vector3 temp = new Vector3(-CreateOctree.ground.transform.position.z, CreateOctree.ground.transform.position.z / 5.0f, CreateOctree.ground.transform.position.z);
            cam.transform.position = temp;
            once = false;
        }
    }
    public void destroyPig(PigBase pig)
    {
        engine.removeCullider(pig.gameObject);
        Destroy(pig.gameObject);
        destroyedPigs++;
        // if (destroyedPigs == pigsCount)
        // {
        //     gameOver();
        // }
    }
    public void destroyFC(FCObject fcObj)
    {
        engine.removeCullider(fcObj.gameObject);
        Destroy(fcObj.gameObject);
    }
    public void destroyMulti(MultiObstacleBase multi)
    {
        Cullider[] culliders = multi.children;
        foreach (Cullider cullider in culliders)
        {
            VoxelCullider voxelCullider = (VoxelCullider)cullider;
            engine.removeCullider(voxelCullider.gameObject);
        }
        Destroy(multi.gameObject);
    }
    private void gameOver()
    {
        SceneManager.LoadScene("Gameover");
    }
}
