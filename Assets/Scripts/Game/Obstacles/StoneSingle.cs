using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSingle : SingleObstacleBase
{
    private static int damMulti = 2;
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver().tag == "Egg")
        {
            FCObject otherFC = (FCObject)other.getRigidbodyDriver();
            health -= ((otherFC.getDamage() + getDamage()) / 2) * damMulti;
            if (health <= 0) levelCtrlr.destroyFC(this);
            return;
        }
        base.onCullisionEnter(other);
    }
}
