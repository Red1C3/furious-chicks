using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCullider : MonoBehaviour, Cullider
{
    private float radius;
    private Vector3 center;

    void Start()
    {
        radius = transform.localScale.x / 2.0f;
        center = transform.position;
    }

    void FixedUpdate()
    {
        center = transform.position;
    }

    public CullisionInfo cullideWith(Cullider other)
    {
        if (other is SphereCullider)
        {
            SphereCullider otherSphere = other as SphereCullider;

            float disBetweenCenters = Vector3.Distance(center, otherSphere.center);
            if (disBetweenCenters > radius + otherSphere.radius)
            {
                return new CullisionInfo(false, Vector3.zero, 0, false, false, Vector3.zero, Vector3.zero);
            }
            else
            {
                float depth = radius + otherSphere.radius - disBetweenCenters;
                Vector3 normal = (center - otherSphere.center).normalized;

                return new CullisionInfo(true, normal, depth, false, false, Vector3.zero, Vector3.zero);
            }
        }
        if(other is BoxCullider){
            return cullideWithBox(other as BoxCullider);
        }
        return new CullisionInfo(false, Vector3.zero, 0, false, false, Vector3.zero, Vector3.zero);
    }

    public CullisionInfo cullideWithBox(BoxCullider other){
        Transform boxTransform=other.transform;
        Vector3 localSphereCenter=boxTransform.InverseTransformPoint(center);
        Vector3 mtv;
        Vector3 contactPoint;
        
        Vector3 closest=new Vector3(
            Mathf.Max(-0.5f,Mathf.Min(localSphereCenter.x,0.5f)),
            Mathf.Max(-0.5f,Mathf.Min(localSphereCenter.y,0.5f)),
            Mathf.Max(-0.5f,Mathf.Min(localSphereCenter.z,0.5f))
        );


        Vector3 offset=localSphereCenter-closest;
        if(boxTransform.TransformVector(offset).magnitude<radius){
            mtv=(radius-boxTransform.TransformVector(offset).magnitude)*boxTransform.TransformVector(offset).normalized;
            contactPoint=boxTransform.TransformPoint(closest);
            if(mtv==Vector3.zero){
                Vector3 closestFacePoint=BoxCullider.clampToClosestFace(localSphereCenter);
                Vector3 penetration=localSphereCenter-closestFacePoint;

                mtv=-1*(radius+boxTransform.TransformVector(penetration).magnitude)*boxTransform.TransformVector(penetration).normalized;
                contactPoint=boxTransform.TransformPoint(closestFacePoint);
            }

            return new CullisionInfo(true,mtv.normalized,mtv.magnitude,false,true,Vector3.zero,contactPoint);
        }else{
            return CullisionInfo.NO_CULLISION;
        }
    }
}
