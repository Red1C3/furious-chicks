using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBird : BirdBase
{
    protected override void ability(){
        addForce(velocity*100,ForceMode.Impulse);
    }
}
