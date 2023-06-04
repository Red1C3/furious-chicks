using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBase : RigidbodyDriver
{
    private static float timeTillDeath = 2.0f;
    private static float linearStoppingThreshold = 0.1f;
    private static float angularStoppingThreshold = 0.1f;
    private float stoppedTimestamp;
    private bool hasStopped = false;
    public bool hasFired = false;
    protected static int firstDamage = 20; //speed=0-2
    protected static int secondDamage = 50; //speed=2-20
    protected static int thirdDamage = 100; //speed>20
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
    public int getDamage()
    {
        float speed = velocity.magnitude;
        if (speed < 2)
        {
            return firstDamage;
        }
        else if (speed < 20)
        {
            return secondDamage;
        }
        else
        {
            return thirdDamage;
        }
    }
}
