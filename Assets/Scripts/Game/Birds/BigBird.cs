using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBird : BirdBase
{
   protected override int firstDamage()
    {
        return 50;
    }
    protected override int secondDamage()
    {
        return 100;
    }
}
