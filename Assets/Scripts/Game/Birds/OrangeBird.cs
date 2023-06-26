using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeBird : BirdBase
{
    float maxScale, updateRate;
    protected override void Start()
    {
        base.Start();
        maxScale = transform.localScale.x;
        updateRate = 1.0e-1f;
    }
    protected override void ability()
    {
        maxScale *= 10;
    }

    new protected void Update()
    {
        base.Update();
        if (!hasAbility && transform.localScale.x <= maxScale)
        {
            transform.localScale += new Vector3(1, 1, 1) * updateRate;
            float newMass = mass + updateRate * mass;
            if (!LevelCtrlr.spaceLvl)
                addForce(gravity * updateRate * mass, ForceMode.Force);
            mass = newMass;
            levelCtrlr.currentBirdThrow.cameraAway += 2 * updateRate;
            GetComponent<SphereCullider>().updateRadius();
        }
    }
    protected override int firstDamage()
    {
        return 5;
    }
    protected override int secondDamage()
    {
        return 20;
    }
}
