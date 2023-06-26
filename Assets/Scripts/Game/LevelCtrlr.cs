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
    private GameObject linePrefab, parabolaPrefab;
    public static LineRenderer line { get; private set; }
    public static LineRenderer parabola { get; private set; }
    public Throw currentBirdThrow { get; private set; }
    public Engine engine { get; private set; }
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

    public static float MovementSmoothingValue = 25f;
    public static Vector3 currentVelocity = Vector3.zero;
    private CameraMovement cameraMovement;
    public static bool spaceLvl { get; private set; }

    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        parabola = Instantiate(parabolaPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        engine = FindObjectOfType<Engine>();
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
        cameraMovement = cam.GetComponent<CameraMovement>();
        playerView = true;
        once = true;

        if (createlvlUI)
        {
            Instantiate(lvlUiPrefab, FindObjectOfType<Canvas>().transform);
        }
        spaceLvl = FindObjectOfType<SpaceRandomGen>() != null;
    }


    private void Update()
    {
        if (throwingPhase)
        {
            currentBirdThrow = currentBird.gameObject.GetComponent<Throw>();
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
                    //Should end game but got removed for demoenstration reasons
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

        Vector3 playerViewPos = new Vector3(currentBird.transform.position.x, currentBird.transform.position.y, -(currentBirdThrow.force + currentBirdThrow.cameraAway));
        if (playerView && once)
        {
            currentRotation.eulerAngles = Vector3.zero;
            cam.transform.rotation = currentRotation;

            cam.transform.position = playerViewPos;
            once = false;
        }
        else if (!playerView && once)
        {
            cameraMovement.FollowDistance = Engine.ground.transform.position.magnitude;
            once = false;
        }
        else if (!playerView)
        {
            cameraMovement.Move();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
    public void destroyPig(PigBase pig)
    {
        engine.removeCullider(pig.gameObject);
        Destroy(pig.gameObject);
        destroyedPigs++;
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
}
