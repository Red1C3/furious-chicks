using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCObject : RigidbodyDriver
{
    protected LevelCtrlr levelCtrlr;
    protected static int firstDamage = 20; //speed=0-2
    protected static int secondDamage = 50; //speed=2-20
    protected static int thirdDamage = 100; //speed>20
    protected override void Start()
    {
        base.Start();
        levelCtrlr = FindObjectOfType<LevelCtrlr>();
    }
    public int getDamage()
    {
        float speed = velocity.magnitude;
        if (speed < 10)
        {
            return firstDamage;
        }
        else if (speed < 50)
        {
            return secondDamage;
        }
        else
        {
            return thirdDamage;
        }
    }
}
