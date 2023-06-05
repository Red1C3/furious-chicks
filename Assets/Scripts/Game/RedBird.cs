using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBird : BirdBase
{
    bool once=true;
    protected void ability(){
        addForce(velocity*100,ForceMode.Impulse);
    }

    public void Update(){
        if(hasFired){
            Debug.Log(velocity.z);
            if(Input.GetKeyDown(KeyCode.F)){
                Debug.Log("pressed");
                ability();
                Debug.Log("pressed");
            }
        }
    }
}
