using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCtrlr : MonoBehaviour
{
    [SerializeField]
    private BirdBase[] birds;
    private int currentBird = 0;
    private bool throwingPhase = true;
    [SerializeField]
    private GameObject linePrefab;
    private LineRenderer line;
    private Throw currentBirdThrow;
    public CreateOctree engine { get; private set; }
    private int pigsCount;
    private int destroyedPigs = 0;
    public static bool playerView {get; private set;}
    private bool once;
    public static Camera cam;
    Quaternion currentRotation;
    Vector3 currentEulerAngles;

    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        engine = FindObjectOfType<CreateOctree>();
        pigsCount = FindObjectsOfType<PigBase>().Length;
        engine.setPlayer(birds[currentBird].gameObject);
        cam = FindObjectOfType<Camera>();
        playerView = true;
        once = true;
    }


    private void Update()
    {
        if (throwingPhase)
        {
            currentBirdThrow = birds[currentBird].gameObject.AddComponent<Throw>();
            currentBirdThrow.lineRenderer = line;
            throwingPhase = false;
        }
        else if (currentBirdThrow.hasFired())
        {
            birds[currentBird].hasFired = true;
            if (birds[currentBird].isDead())
            {
                Destroy(birds[currentBird].gameObject);
                if (currentBird + 1 == birds.Length)
                {
                    gameOver();
                }
                else
                {
                    currentBird++;
                    engine.setPlayer(birds[currentBird].gameObject);
                    throwingPhase = true;
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.C)){
            playerView=!playerView;
            once = true;
        }
        
        if(playerView && once){
            currentRotation.eulerAngles = Vector3.zero;
            cam.transform.rotation = currentRotation;

            cam.transform.position = new Vector3(birds[currentBird].transform.position.x, birds[currentBird].transform.position.y, -(currentBirdThrow.force+currentBirdThrow.cameraAway));
            once=false;
        }
        else if(once){
            currentRotation.eulerAngles = new Vector3(0,90,0);
            cam.transform.rotation = currentRotation;
            Vector3 temp = new Vector3(-CreateOctree.maxZ/2.0f,CreateOctree.maxZ/10.0f,CreateOctree.maxZ/2.0f);
            cam.transform.position = temp;
            once=false;
        }
    }
    public void destroyPig(PigBase pig)
    {
        engine.removeCullider(pig.gameObject);
        Destroy(pig.gameObject);
        destroyedPigs++;
        if (destroyedPigs == pigsCount)
        {
            gameOver();
        }
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
