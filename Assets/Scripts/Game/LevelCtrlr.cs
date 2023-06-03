using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCtrlr : MonoBehaviour
{
    [SerializeField]
    private BirdBase[] birds;
    private int currentBird = 0;
    private bool throwingPhase = true;
    [SerializeField]
    private GameObject linePrefab;
    private LineRenderer line;

    private void Start()
    {
        line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity).GetComponent<LineRenderer>();
    }


    private void Update()
    {
        if (throwingPhase)
        {
            Throw throwInst = birds[currentBird].gameObject.AddComponent<Throw>();
            throwInst.lineRenderer = line;
            throwInst.cam = Camera.main;
            throwingPhase=false;
        }
        else
        {
            //Check if currentBird is dead, if dead check pigs count, if zero end level
            //if not zero increment current bird, destroy previous bird and start throwing
        }
    }
}
