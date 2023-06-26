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
        if (LevelCtrlr.spaceLvl)
        {
            egg.GetComponent<RigidbodyDriver>().useGravity = false;
        }
        levelCtrlr.engine.addCullider(egg);
    }

    private void OnDestroy()
    {
        if (egg != null)
        {
            levelCtrlr.destroyFC(egg.GetComponent<FCObject>());
        }
    }
    protected override int firstDamage()
    {
        return 15;
    }
    protected override int secondDamage()
    {
        return 30;
    }
}
