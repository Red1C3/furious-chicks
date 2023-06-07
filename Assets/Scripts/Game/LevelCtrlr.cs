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

    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        engine = FindObjectOfType<CreateOctree>();
        pigsCount = FindObjectsOfType<PigBase>().Length;
        engine.setPlayer(birds[currentBird].gameObject);
    }


    private void Update()
    {
        if (throwingPhase)
        {
            currentBirdThrow = birds[currentBird].gameObject.AddComponent<Throw>();
            currentBirdThrow.lineRenderer = line;
            currentBirdThrow.cam = Camera.main;
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
