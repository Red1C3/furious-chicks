using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBird : BirdBase
{
    [SerializeField]
    private GameObject eggPrefab;

    private GameObject egg;
    protected override void ability(){
        Vector3 eggPos = transform.position + new Vector3(0.0f,-0.5f,0.2f);
        egg = Instantiate(eggPrefab, eggPos, Quaternion.identity);
        levelCtrlr.engine.addCullider(egg);
    }
}
