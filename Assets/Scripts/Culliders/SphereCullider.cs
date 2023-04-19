using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCullider : MonoBehaviour,Cullider
{
    public bool copyFromTransform;
    public float radius;
    private Vector3 center;

    void Start(){
        if(copyFromTransform){
            radius=transform.localScale.x/2.0f;
        }
        center=transform.position;
    }

    void FixedUpdate(){
        center=transform.position;
    }

    public CullisionInfo cullideWith(Cullider other){
        if(other is SphereCullider){
            SphereCullider otherSphere=other as SphereCullider;

            float disBetweenCenters=Vector3.Distance(center,otherSphere.center);
            if(disBetweenCenters>radius+otherSphere.radius){
                return new CullisionInfo(false,Vector3.zero,0,false,false,Vector3.zero,Vector3.zero);
            }else{
                float depth=radius+otherSphere.radius-disBetweenCenters;
                Vector3 normal=(center-otherSphere.center).normalized;

                return new CullisionInfo(true,normal,depth,false,false,Vector3.zero,Vector3.zero);
            }
        }
        return new CullisionInfo(false,Vector3.zero,0,false,false,Vector3.zero,Vector3.zero);
    }
}
