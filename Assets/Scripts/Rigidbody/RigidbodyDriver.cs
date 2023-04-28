using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    private static Vector3 gravity = new Vector3(0, -9.8f, 0);
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void physicsUpdate()
    {
        if (rb.useGravity)
            velocity += gravity * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    public void applyLinearMomentum(RigidbodyDriver other)
    {
        float alpha = (rb.mass - other.rb.mass) / (rb.mass + other.rb.mass);
        float beta = other.rb.mass / (rb.mass + other.rb.mass);
        float gamma = rb.mass / (rb.mass + other.rb.mass);

        velocity = alpha * velocity + 2 * beta * other.velocity;
        other.velocity = 2 * gamma * velocity - alpha * other.velocity;
    }
}
