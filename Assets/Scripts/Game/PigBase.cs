using UnityEngine;

public class PigBase : FCObject
{
    [SerializeField]
    protected int health = 100;
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is FCObject)
        {
            FCObject otherFC = (FCObject)other.getRigidbodyDriver();
            health -= otherFC.getDamage();
            if (health <= 0) levelCtrlr.destroyPig(this);
        }
    }
}