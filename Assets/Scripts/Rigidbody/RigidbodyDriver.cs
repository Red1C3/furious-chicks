using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    [SerializeField]
    private bool freezePX, freezePY, freezePZ, freezeRX, freezeRY, freezeRZ;
    [SerializeField]
    private bool startFrozen = true;
    public bool psudoFreeze { get; private set; }
    public float mass = 1.0f;
    public bool useGravity = true;
    public Vector3 initialAngularVelocity; //In radians
    public Vector3 initialVelocity;
    private Vector3 _velocity;
    public Vector3 velocity
    {
        get
        {
            Vector3 v = Vector3.zero;
            if (psudoFreeze) return v;
            if (!freezePX) v.x = _velocity.x;
            if (!freezePY) v.y = _velocity.y;
            if (!freezePZ) v.z = _velocity.z;
            return v;
        }
        private set { _velocity = value; }
    }
    private Quaternion _angularVelocity;
    //w component is always 0
    private Quaternion angularVelocity
    {
        get
        {
            Quaternion q = new Quaternion(0, 0, 0, 0);
            if (psudoFreeze) return q;
            if (!freezeRX) q.x = _angularVelocity.x;
            if (!freezeRY) q.y = _angularVelocity.y;
            if (!freezeRZ) q.z = _angularVelocity.z;
            return q;
        }
        set { _angularVelocity = value; }
    }
    public readonly static Vector3 gravity = new Vector3(0, -9.8f, 0);

    private Shape shape;

    public Vector3 acclumatedForces { get; private set; }
    private Vector3 acclumatedImpulses;
    protected virtual void Start()
    {
        if (gameObject.tag != "Player")
            psudoFreeze = startFrozen;
        shape = GetComponent<Shape>();
        angularVelocity = new Quaternion(initialAngularVelocity.x, initialAngularVelocity.y, initialAngularVelocity.z, 0);
        velocity = initialVelocity;
        if (useGravity)
            acclumatedForces += gravity * mass;
    }


    public void applyForces()
    {
        Vector3 frameForces = acclumatedForces + acclumatedImpulses;
        Vector3 acceleration = frameForces / mass;
        velocity += acceleration * Time.fixedDeltaTime;
        acclumatedImpulses = Vector3.zero;
    }

    public void physicsUpdate()
    {
        //Debug.Log(string.Format("{0}, {1}, {2}, {3}",angularVelocity.x,angularVelocity.y,angularVelocity.z,angularVelocity.w));
        //
        //Faulty since lacking Reaction force
        //if (rb.useGravity)
        //    velocity += gravity * Time.fixedDeltaTime;
        //Vector3 appliedVelocity=transform.InverseTransformVector(velocity);

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
        float alpha = (mass - other.mass) / (mass + other.mass);
        float beta = other.mass / (mass + other.mass);
        float gamma = mass / (mass + other.mass);

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
        float3x3 otherInertiaTensor = other.shape.getTensorInertia();

        //Analogus implmentation
        //float3x3 alpha = (inertiaTensor - otherInertiaTensor) * math.inverse(inertiaTensor + otherInertiaTensor);
        //float3x3 beta = otherInertiaTensor * math.inverse(inertiaTensor + otherInertiaTensor);
        //float3x3 gamma = inertiaTensor * math.inverse(inertiaTensor + otherInertiaTensor);

        //float3 newVelocity = math.mul(alpha, vecAngVelocity) + math.mul(2 * beta, otherVecAngVelocity);
        //float3 newOtherVelocity = math.mul(2 * gamma, vecAngVelocity) - math.mul(alpha, otherVecAngVelocity);

        //Issawi's implementation (it looks like it gives the same results as above :/)
        float3 newOtherVelocity = math.mul(math.inverse(inertiaTensor + otherInertiaTensor),
                                math.mul(otherInertiaTensor, otherVecAngVelocity) + math.mul(inertiaTensor, 2 * vecAngVelocity) -
                                math.mul(inertiaTensor, otherVecAngVelocity));

        float3 newVelocity = otherVecAngVelocity + newOtherVelocity - vecAngVelocity;

        angularVelocity = new Quaternion(newVelocity.x, newVelocity.y, newVelocity.z, 0);
        other.angularVelocity = new Quaternion(newOtherVelocity.x, newOtherVelocity.y, newOtherVelocity.z, 0);
    }

    public void addAngularVelocity(Vector3 vector3)
    {
        Quaternion aVelocity = angularVelocity;
        aVelocity.x += vector3.x;
        aVelocity.y += vector3.y;
        aVelocity.z += vector3.z;
        angularVelocity = aVelocity;
    }
    public void addLinearVelocity(Vector3 vector3)
    {
        velocity += vector3;
    }
    public float3x3 getInertiaTensor()
    {
        return shape.getTensorInertia();
    }

    public Vector3 getAngularVelocity()
    {
        return new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z);
    }

    public void addForce(Vector3 force, ForceMode mode)
    {
        switch (mode)
        {
            case ForceMode.Force:
                acclumatedForces += force;
                break;
            case ForceMode.Impulse:
                acclumatedImpulses += force;
                break;
            default:
                Debug.Log("unsupported force mode passed to Rigidbody.addForce");
                break;
        }
    }
    public Vector3 getInverseMassVector3()
    {
        Vector3 inverseMassVector3 = Vector3.zero;
        if (psudoFreeze) return inverseMassVector3;
        if (freezePX && freezePY && freezePZ) return inverseMassVector3;

        float inverseMass = 1.0f / mass;
        if (!freezePX) inverseMassVector3.x = inverseMass;
        if (!freezePY) inverseMassVector3.y = inverseMass;
        if (!freezePZ) inverseMassVector3.z = inverseMass;
        return inverseMassVector3;
    }
    public Vector3 getInverseInertiaVector3(float3 axis)
    {
        Vector3 inverseInertiaVector3 = Vector3.zero;
        if (psudoFreeze) return inverseInertiaVector3;
        if (freezeRX && freezeRY && freezeRZ) return inverseInertiaVector3;

        VoxelGrid grid;
        float inverseInertiaScalar;
        if (TryGetComponent<VoxelGrid>(out grid))
        {
            inverseInertiaScalar = 1.0f / Shape.inertiaScalar(grid.getInertiaTensor(), axis);
        }
        else
        {
            inverseInertiaScalar = 1.0f / Shape.inertiaScalar(getInertiaTensor(), axis);
        }
        if (!freezeRX) inverseInertiaVector3.x = inverseInertiaScalar;
        if (!freezeRY) inverseInertiaVector3.y = inverseInertiaScalar;
        if (!freezeRZ) inverseInertiaVector3.z = inverseInertiaScalar;
        return inverseInertiaVector3;
    }
    public void psudoUnfreeze()
    {
        psudoFreeze = false;
    }
    public virtual void onCullisionEnter(Cullider other){
    }
    public virtual void onCullisionExit(Cullider other){
    }
    public virtual void onCullisionStay(Cullider other){
    }
}
