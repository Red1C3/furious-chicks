using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    //w component is always 0
    private Quaternion angularVelocity;
    private static Vector3 gravity = new Vector3(0, -9.8f, 0);

    private Shape shape;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        shape = GetComponent<Shape>();
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

        Vector3 newVelocity = alpha * velocity + 2 * beta * other.velocity;
        Vector3 newOtherVelocity = 2 * gamma * velocity - alpha * other.velocity;

        velocity = newVelocity;
        other.velocity = newOtherVelocity;
    }

    public void applyAngularMomentum(RigidbodyDriver other)
    {
        float3 vecAngVelocity = new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z);
        float3 otherVecAngVelocity = new Vector3(other.angularVelocity.x,
                                            other.angularVelocity.y, other.angularVelocity.z);


        float3x3 inertiaTensor = shape.getTensorInertia();
        float3x3 otherInertiaTensor = shape.getTensorInertia();

        float3x3 alpha = (inertiaTensor - otherInertiaTensor) * math.inverse(inertiaTensor + otherInertiaTensor);
        float3x3 beta = otherInertiaTensor * math.inverse(inertiaTensor + otherInertiaTensor);
        float3x3 gamma = inertiaTensor * math.inverse(inertiaTensor + otherInertiaTensor);

        float3 newVelocity = math.mul(alpha, vecAngVelocity) + math.mul(2 * beta, otherVecAngVelocity);
        float3 newOtherVelocity = math.mul(2 * gamma, vecAngVelocity) - math.mul(alpha, otherVecAngVelocity);

        angularVelocity = new Quaternion(newVelocity.x, newVelocity.y, newVelocity.z, 0);
        other.angularVelocity = new Quaternion(newOtherVelocity.x, newOtherVelocity.y, newOtherVelocity.z, 0);
    }
}
