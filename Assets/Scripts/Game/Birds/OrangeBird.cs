using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeBird : BirdBase
{
    float maxScale, updateRate;
    protected override void Start(){
        base.Start();
        maxScale=transform.localScale.x;
        updateRate=1.0e-2f;
    }
    protected override void ability()
    {
        maxScale*=4;
    }

    protected void Update()
    {
        base.Update();
        if(!hasAbility && transform.localScale.x<=maxScale){
            transform.localScale += new Vector3(1, 1, 1) * updateRate;
            mass += 1 * updateRate * mass;
            levelCtrlr.currentBirdThrow.cameraAway += 2 * updateRate;
        }
    }
}
