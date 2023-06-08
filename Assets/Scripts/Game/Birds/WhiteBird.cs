using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBird : BirdBase
{
    [SerializeField]
    private GameObject eggPrefab;

    private GameObject egg;
    protected override void ability()
    {
        Vector3 eggPos = transform.position + (-0.5f * transform.up + 0.2f * transform.forward);
        egg = Instantiate(eggPrefab, eggPos, Quaternion.identity);
        levelCtrlr.engine.addCullider(egg);
    }
}
