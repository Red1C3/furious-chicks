using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBird : BirdBase
{
    [SerializeField]
    private GameObject birdPrefab;
    private GameObject child;
    protected override void ability()
    {
        child = Instantiate(birdPrefab, transform.position + 0.5f * transform.right,
         Quaternion.identity);
        child.GetComponent<Throw>().enabled = false;
        child.GetComponent<RigidbodyDriver>().initialVelocity = velocity;
        child.GetComponent<RigidbodyDriver>().initialAngularVelocity = getAngularVelocity();
        levelCtrlr.engine.addCullider(child);
        child = Instantiate(birdPrefab, transform.position - 0.5f * transform.right,
         Quaternion.identity);
        child.GetComponent<Throw>().enabled = false;
        child.GetComponent<RigidbodyDriver>().initialVelocity = velocity;
        child.GetComponent<RigidbodyDriver>().initialAngularVelocity = getAngularVelocity();
        levelCtrlr.engine.addCullider(child);
    }
    //TODO is dead
}
