using UnityEngine;

public class PigBase : RigidbodyDriver
{
    [SerializeField]
    protected int health = 100;
    public bool toBeDeleted = false;
    private LevelCtrlr levelCtrlr;
    protected override void Start()
    {
        base.Start();
        levelCtrlr = FindObjectOfType<LevelCtrlr>();
    }
    public override void onCullisionEnter(Cullider other)
    {
        if (other.getRigidbodyDriver() is BirdBase)
        {
            BirdBase bird = (BirdBase)other.getRigidbodyDriver();
            health -= bird.getDamage();
            if (health <= 0) levelCtrlr.destroyPig(this);
        }
    }
}