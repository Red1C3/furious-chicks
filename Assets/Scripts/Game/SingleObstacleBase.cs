using UnityEngine;

public class SingleObstacleBase : FCObject
{
    [SerializeField]
    protected int health = 100;
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is FCObject)
        {
            FCObject otherFC = (FCObject)other.getRigidbodyDriver();
            health -= (otherFC.getDamage() + getDamage()) / 2;
            if (health <= 0) levelCtrlr.destroyFC(this);
        }
    }

    protected override int firstDamage()
    {
        return 0;
    }

}