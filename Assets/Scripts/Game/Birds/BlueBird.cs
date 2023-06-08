using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBird : BirdBase
{
    [SerializeField]
    private GameObject birdPrefab;
    private GameObject[] children;
    protected override void Start()
    {
        base.Start();
        children = null;
    }
    protected override void ability()
    {
        children = new GameObject[2];

        children[0] = Instantiate(birdPrefab, transform.position + 0.5f * transform.right,
         Quaternion.identity);
        children[0].GetComponent<Throw>().enabled = false;
        children[0].GetComponent<RigidbodyDriver>().initialVelocity = velocity;
        children[0].GetComponent<RigidbodyDriver>().initialAngularVelocity = getAngularVelocity();
        levelCtrlr.engine.addCullider(children[0]);

        children[1] = Instantiate(birdPrefab, transform.position - 0.5f * transform.right,
         Quaternion.identity);
        children[1].GetComponent<Throw>().enabled = false;
        children[1].GetComponent<RigidbodyDriver>().initialVelocity = velocity;
        children[1].GetComponent<RigidbodyDriver>().initialAngularVelocity = getAngularVelocity();
        levelCtrlr.engine.addCullider(children[1]);
    }
    public override bool isDead()
    {
        if (children == null)
        {
            return base.isDead();
        }
        else
        {
            return base.isDead() &&
                    children[0].GetComponent<BlueBird>().isDead() &&
                    children[1].GetComponent<BlueBird>().isDead();
        }
    }
    private void OnDestroy(){
        if(children!=null){
            levelCtrlr.destroyFC(children[0].GetComponent<FCObject>());
            levelCtrlr.destroyFC(children[1].GetComponent<FCObject>());
        }
    }
}
