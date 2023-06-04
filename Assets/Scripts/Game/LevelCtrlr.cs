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
    private bool isOver = false;
    private CreateOctree engine;

    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
        engine = FindObjectOfType<CreateOctree>();
    }


    private void Update()
    {
        if (isOver) return;

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
                //if all pigs are dead, game over
                //else...
                Destroy(birds[currentBird].gameObject);
                if (currentBird + 1 == birds.Length)
                {
                    isOver = true;
                    SceneManager.LoadScene("Gameover");
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
}
