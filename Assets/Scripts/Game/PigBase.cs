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
            health -= (otherFC.getDamage() + getDamage()) / 2;
            if (health <= 0) levelCtrlr.destroyPig(this);
        }
    }
    protected void Update(){
        if(shape.center().y<CreateOctree.ground.transform.position.y-10.0f)
            levelCtrlr.destroyPig(this);
    }
    protected override int firstDamage()
    {
        return 0;
    }
}