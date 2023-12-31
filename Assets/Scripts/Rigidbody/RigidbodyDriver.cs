using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RigidbodyDriver : MonoBehaviour
{
    [SerializeField]
    private bool drag, angularDrag;
    [SerializeField]
    private bool freezePX, freezePY, freezePZ, freezeRX, freezeRY, freezeRZ;
    [SerializeField]
    private bool startFrozen = true;
    public bool psudoFreeze { get; private set; }
    private static float linearDragVal = 0.001f, angularDragVal = 0.01f;
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

    protected Shape shape;

    public Vector3 acclumatedForces { get; private set; }
    private Vector3 acclumatedImpulses;
    public VoxelGrid voxelGrid { get; protected set; }
    protected virtual void Start()
    {
        if (gameObject.tag != "Player")
            psudoFreeze = startFrozen;
        shape = GetComponent<Shape>();
        voxelGrid = GetComponent<VoxelGrid>();
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
        transform.position += velocity * Time.fixedDeltaTime;

        transform.rotation = quatQuatAdd(transform.rotation,
                            floatQuatMult(0.5f * Time.fixedDeltaTime,
                            (angularVelocity * transform.rotation)));

        if (drag)
        {
            velocity = velocity - velocity * linearDragVal;
        }
        if (angularDrag)
        {
            angularVelocity = quatQuatAdd(angularVelocity, floatQuatMult(-angularDragVal, angularVelocity));
        }

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

        float inverseInertiaScalar;
        if (voxelGrid != null)
        {
            inverseInertiaScalar = 1.0f / Shape.inertiaScalar(voxelGrid.getInertiaTensor(), axis);
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
        if (freezePX && freezePY && freezePZ && freezeRX && freezeRY && freezeRZ) return;
        psudoFreeze = false;
    }
    public virtual void onCullisionEnter(Cullider other)
    {
    }
    public virtual void onCullisionExit(Cullider other)
    {
    }
    public virtual void onCullisionStay(Cullider other)
    {
    }
    public bool getDrag()
    {
        return drag;
    }
    public bool getAngularDrag()
    {
        return angularDrag;
    }
    public bool getFreezePX()
    {
        return freezePX;
    }
    public bool getFreezePY()
    {
        return freezePY;
    }
    public bool getFreezePZ()
    {
        return freezePZ;
    }
    public bool getFreezeRX()
    {
        return freezeRX;
    }
    public bool getFreezeRY()
    {
        return freezeRY;
    }
    public bool getFreezeRZ()
    {
        return freezeRZ;
    }
    public void setDrag(bool val)
    {
        drag = val;
    }
    public void setAngDrag(bool val)
    {
        angularDrag = val;
    }
    public void setFreezePX(bool val)
    {
        freezePX = val;
    }
    public void setFreezePY(bool val)
    {
        freezePY = val;
    }
    public void setFreezePZ(bool val)
    {
        freezePZ = val;
    }
    public void setFreezeRX(bool val)
    {
        freezeRX = val;
    }
    public void setFreezeRY(bool val)
    {
        freezeRY = val;
    }
    public void setFreezeRZ(bool val)
    {
        freezeRZ = val;
    }
    public void setPsudoFreeze(bool val)
    {
        psudoFreeze = val;
    }
    public void setGravity(bool val)
    {
        useGravity = val;
        if (GetComponent<Throw>() != null && !GetComponent<BirdBase>().hasFired) return;
        if (val)
        {
            acclumatedForces += gravity * mass;
        }
        else
        {
            acclumatedForces -= gravity * mass;
        }
    }
    public void setMass(float val)
    {
        if (voxelGrid != null) return;
        float newMass = val;
        if (useGravity)
            addForce(gravity * (newMass - mass), ForceMode.Force);
        mass = newMass;
        BoxCullider box;
        SphereCullider sphere;
        if (TryGetComponent<BoxCullider>(out box))
        {
            box.updateLocalInertiaTensor();
        }
        if (TryGetComponent<SphereCullider>(out sphere))
        {
            sphere.updateLocalInertiaTensor();
        }
    }
}
