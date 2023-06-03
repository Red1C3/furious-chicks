using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCtrlr : MonoBehaviour
{
    [SerializeField]
    private BirdBase[] birds;
    private int currentBird = 0;
    private bool throwingPhase = true;

    private void Update()
    {
        if (throwingPhase)
        {
            //TODO
        }
        else
        {
            //Check if currentBird is dead, if dead check pigs count, if zero end level
            //if not zero increment current bird, destroy previous bird and start throwing
        }
    }
}
