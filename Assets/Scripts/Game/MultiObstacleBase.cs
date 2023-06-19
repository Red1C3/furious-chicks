using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiObstacleBase : FCObject
{
    [SerializeField]
    protected int health = 100;
    public Cullider[] children { get; private set; }
    protected override void Start()
    {
        base.Start();
        children = GetComponentsInChildren<Cullider>();
    }
    protected void Update()
    {
        if (voxelGrid.getVoxelsCenter().y < CreateOctree.ground.transform.position.y - 10.0f)
            levelCtrlr.destroyMulti(this);
    }
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is FCObject)
        {
            FCObject otherFC = (FCObject)other.getRigidbodyDriver();
            health -= (otherFC.getDamage() + getDamage()) / 2;
            if (health <= 0) levelCtrlr.destroyMulti(this);
        }
    }
    protected override int firstDamage()
    {
        return 0;
    }
    public void setHealth(int value){
        health=value;
    }
    public int getHealth(){
        return health;
    }
}
