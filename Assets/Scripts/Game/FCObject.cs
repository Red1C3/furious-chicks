using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCObject : RigidbodyDriver
{
    protected LevelCtrlr levelCtrlr;
    protected override void Start()
    {
        base.Start();
        levelCtrlr = FindObjectOfType<LevelCtrlr>();
    }
    public int getDamage()
    {
        float speed = velocity.magnitude;
        if (speed < 1.5f)
        {
            return firstDamage();
        }
        else if (speed < 30)
        {
            return secondDamage();
        }
        else
        {
            return thirdDamage();
        }
    }
    protected virtual int firstDamage(){
        return 20;
    }
    protected virtual int secondDamage(){
        return 50;
    }
    protected virtual int thirdDamage(){
        return 100;
    }
}
