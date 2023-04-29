using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    //w component is always 0
    private Quaternion angularVelocity;
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

        transform.rotation = quatQuatAdd(transform.rotation,
                            floatQuatMult(0.5f * Time.fixedDeltaTime,
                            (angularVelocity * transform.rotation)));

    }

    public static Quaternion floatQuatMult(float f, Quaternion quat)
    {
        quat[0] *= f;
        quat[1] *= f;
        quat[2] *= f;
        quat[3] *= f;
        return quat;
    }

    public static Quaternion quatQuatAdd(Quaternion quat0, Quaternion quat1)
    {
        quat0[0] += quat1[0];
        quat0[1] += quat1[1];
        quat0[2] += quat1[2];
        quat0[3] += quat1[3];
        return quat0;
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
