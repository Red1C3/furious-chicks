using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBase : RigidbodyDriver
{
    private static float timeTillDeath = 2.0f;
    private static float linearStoppingThreshold = 0.01f;
    private static float angularStoppingThreshold = 0.001f;
    private float stoppedTimestamp;
    private bool hasStopped = false;
    public bool hasFired = false;
    protected override void Start()
    {
        base.Start();
        stoppedTimestamp = Time.time;
    }

    protected void Update()
    {
        if (hasFired)
        {
            if (Time.time - stoppedTimestamp >= timeTillDeath)
            {
                hasStopped = true;
            }
            if (!stopped())
            {
                stoppedTimestamp = Time.time;
                hasStopped = false;
            }
        }
        else
        {
            stoppedTimestamp = Time.time;
        }
    }
    private bool stopped()
    {
        return velocity.magnitude < linearStoppingThreshold &&
                getAngularVelocity().magnitude < angularStoppingThreshold;
    }
    public bool isDead()
    {
        return hasStopped; //Maybe if bird beneath ground level too
    }
}
