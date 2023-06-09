using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SphereCullider : MonoBehaviour, Cullider
{
    [SerializeField]
    private float frictionCo = 0.0f;
    [SerializeField]
    private float bouncinessCo = 0.0f;
    private float radius;
    private Vector3 center;
    private RigidbodyDriver rigidbodyDriver;
    private HashSet<Cullider> frameCulliders, stayedCulliders;

    void Start()
    {
        frameCulliders = new HashSet<Cullider>();
        stayedCulliders = new HashSet<Cullider>();
        rigidbodyDriver = GetComponent<RigidbodyDriver>();
        radius = transform.localScale.x / 2.0f;
        center = transform.position;
    }

    void FixedUpdate()
    {
        center = transform.position;
    }

    public Bounds getBounds()
    {
        //REQUIRES mesh, otherwise bounds oughta be calculated manually
        return GetComponent<Renderer>().bounds;
    }
    public CullisionInfo cullideWith(Cullider other)
    {
        if (other is SphereCullider)
        {
            SphereCullider otherSphere = other as SphereCullider;

            float disBetweenCenters = Vector3.Distance(center, otherSphere.center);
            if (disBetweenCenters > radius + otherSphere.radius)
            {
                return CullisionInfo.NO_CULLISION;
            }
            else
            {
                float depth = radius + otherSphere.radius - disBetweenCenters;
                //From other sphere to base
                Vector3 normal = (center - otherSphere.center).normalized;

                Vector3 contactPointA = center + (-normal * radius);

                Vector3 contactPointB = otherSphere.center + normal * otherSphere.radius;

                Vector3 contactPoint = (contactPointA + contactPointB) / 2.0f;

                return new CullisionInfo(true, normal, depth, true, true, contactPoint, contactPoint, this, other);
            }
        }
        if (other is BoxCullider)
        {
            return cullideWithBox(other as BoxCullider);
        }
        return CullisionInfo.NO_CULLISION;
    }

    public CullisionInfo cullideWithBox(BoxCullider other)
    {
        Transform boxTransform = other.transform;
        Vector3 localSphereCenter = boxTransform.InverseTransformPoint(center);
        Vector3 mtv;
        Vector3 contactPoint;

        Vector3 closest = new Vector3(
            Mathf.Max(-0.5f, Mathf.Min(localSphereCenter.x, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(localSphereCenter.y, 0.5f)),
            Mathf.Max(-0.5f, Mathf.Min(localSphereCenter.z, 0.5f))
        );


        Vector3 offset = localSphereCenter - closest;
        if (boxTransform.TransformVector(offset).magnitude < radius)
        {
            //From other to base
            mtv = (radius - boxTransform.TransformVector(offset).magnitude) * boxTransform.TransformVector(offset).normalized;
            contactPoint = boxTransform.TransformPoint(closest);
            if (mtv == Vector3.zero)
            {
                Vector3 closestFacePoint = BoxCullider.clampToClosestFace(localSphereCenter);
                Vector3 penetration = localSphereCenter - closestFacePoint;

                mtv = -1 * (radius + boxTransform.TransformVector(penetration).magnitude) * boxTransform.TransformVector(penetration).normalized;
                contactPoint = boxTransform.TransformPoint(closestFacePoint);
            }

            Vector3 sphereContactPoint = -mtv.normalized * radius + center;

            return new CullisionInfo(true, mtv.normalized, mtv.magnitude, true, true, sphereContactPoint, contactPoint, this, other);
        }
        else
        {
            return CullisionInfo.NO_CULLISION;
        }
    }

    public float3x3 getTensorInertia()
    {
        float diag = (2.0f / 5.0f) * rigidbodyDriver.mass * radius * radius;

        float3x3 tensor = float3x3.identity;

        tensor[0][0] = diag;
        tensor[1][1] = diag;
        tensor[2][2] = diag;

        return tensor;
    }

    Vector3 Shape.center()
    {
        return center;
    }

    public RigidbodyDriver getRigidbodyDriver()
    {
        return GetComponent<RigidbodyDriver>();
    }

    public float getFrictionCo()
    {
        return frictionCo;
    }
    public float getBouncinessCo()
    {
        return bouncinessCo;
    }
    public HashSet<Cullider> getFrameCulliders()
    {
        return frameCulliders;
    }
    public HashSet<Cullider> getStayedCulliders()
    {
        return stayedCulliders;
    }

    public void updateRadius()
    {
        radius = transform.localScale.x / 2.0f;
        //FIXME after megring optimzation-2 do recalcuate inertia tensor
    }
}
