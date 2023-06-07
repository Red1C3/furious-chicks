using UnityEngine;

public class PigBase : FCObject
{
    [SerializeField]
    protected int health = 100;
    protected static new int firstDamage = 0;
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is FCObject)
        {
            FCObject otherFC = (FCObject)other.getRigidbodyDriver();
            health -= (otherFC.getDamage() + getDamage()) / 2;
            if (health <= 0) levelCtrlr.destroyPig(this);
        }
    }
}