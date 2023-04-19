using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCullider : MonoBehaviour,Cullider
{
    private Vector3 center;
    private Vector3 size;
    private Quaternion rotation;

    private Vector3[] vertices;
    private Vector3 right, up, forward;

    void Start()
    {
        updateBoundaries();
    }

    void FixedUpdate(){
        updateBoundaries();
    }

    public CullisionInfo cullideWith(Cullider other){
        if(other is BoxCullider){
            return cullideWithBox(other as BoxCullider);
        }
        return CullisionInfo.NO_CULLISION;
    }

    //TODO if of the same voxel grid return no collision
    private CullisionInfo cullideWithBox(BoxCullider other){
        float overlap = float.MaxValue;
        Vector3 axis = Vector3.zero;
        float tempOverlap;

        if ((tempOverlap = calculateOverlap(other,right)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = right;
        }
        if ((tempOverlap = calculateOverlap(other,up)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = up;
        }
        if ((tempOverlap = calculateOverlap(other,forward)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = forward;
        }
        if ((tempOverlap = calculateOverlap(other,other.right))< 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.right;
        }
        if ((tempOverlap = calculateOverlap(other,other.up)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.up;
        }
        if ((tempOverlap = calculateOverlap(other,other.forward)) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = other.forward;
        }
        if ((tempOverlap =calculateOverlap(other,Vector3.Cross(right,other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.right);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(right,other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.up);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(right,other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(right, other.forward);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(up,other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up,other.right);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(up,other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up, other.up);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(up,other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(up, other.forward);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(forward,other.right))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.right);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(forward,other.up))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.up);
        }
        if ((tempOverlap = calculateOverlap(other,Vector3.Cross(forward,other.forward))) < 0)
            return CullisionInfo.NO_CULLISION;
        else if (tempOverlap < overlap)
        {
            overlap = tempOverlap;
            axis = Vector3.Cross(forward, other.forward);
        }
        return new CullisionInfo(true,fixAxis(other,overlap,axis),overlap,false,false,Vector3.zero,Vector3.zero);
    }

    private float calculateOverlap(BoxCullider other,Vector3 axis){
        // Handles the cross product = {0,0,0} case
        // Infinite cuz we'll take the smallest overlap 
        if (axis == Vector3.zero)
            return float.MaxValue;

        var aMin = float.MaxValue;
        var aMax = float.MinValue;
        var bMin = float.MaxValue;
        var bMax = float.MinValue;

        // Define two intervals, a and b. Calculate their min and max values
        for (var i = 0; i < 8; i++)
        {
            var aDist = Vector3.Dot(vertices[i], axis);
            aMin = aDist < aMin ? aDist : aMin;
            aMax = aDist > aMax ? aDist : aMax;
            var bDist = Vector3.Dot(other.vertices[i], axis);
            bMin = bDist < bMin ? bDist : bMin;
            bMax = bDist > bMax ? bDist : bMax;
        }

        // One-dimensional intersection test between a and b
        var longSpan = Mathf.Max(aMax, bMax) - Mathf.Min(aMin, bMin);
        var sumSpan = aMax - aMin + bMax - bMin;
        if (longSpan >= sumSpan)
        {
            return -1.0f;
        }
        else
        {
            return sumSpan - longSpan;
        }
    }


    void updateBoundaries(){
        
        center = transform.position;
        size = transform.localScale;
        rotation = transform.rotation;

        var max = size / 2;
        var min = -max;

        vertices = new[]
            {
                center + rotation * min,
                center + rotation * new Vector3(max.x, min.y, min.z),
                center + rotation * new Vector3(min.x, max.y, min.z),
                center + rotation * new Vector3(max.x, max.y, min.z),
                center + rotation * new Vector3(min.x, min.y, max.z),
                center + rotation * new Vector3(max.x, min.y, max.z),
                center + rotation * new Vector3(min.x, max.y, max.z),
                center + rotation * max,
           };

        right = rotation * Vector3.right;
        up = rotation * Vector3.up;
        forward=rotation*Vector3.forward;
    }

    private Vector3 fixAxis(BoxCullider other,float depth,Vector3 axis){
        Vector3 pos=transform.position;
        transform.position+=axis.normalized*depth;
        if(calculateOverlap(other,axis)<depth){
            transform.position=pos;
            return axis;
        }else{
            transform.position=pos;
            return -axis;
        }
    }
}
