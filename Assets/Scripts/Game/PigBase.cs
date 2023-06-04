using UnityEngine;

public class PigBase : RigidbodyDriver
{
    [SerializeField]
    protected int health = 100;
    public bool toBeDeleted = false;

    protected void destroy()
    {
        //Destroy(gameObject); breaks octree
    }
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is BirdBase)
        {
            BirdBase bird = (BirdBase)other.getRigidbodyDriver();
            health -= bird.getDamage();
            Debug.Log(health);
            if (health <= 0) destroy();
        }
    }
}