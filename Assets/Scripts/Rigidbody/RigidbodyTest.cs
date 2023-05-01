using UnityEngine;
public class RigidbodyTest:MonoBehaviour{
    public Vector3 velocity;
    void Start(){
        GetComponent<Rigidbody>().velocity=velocity;
    }
}