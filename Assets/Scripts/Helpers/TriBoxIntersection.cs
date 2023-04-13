using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriBoxIntersection
{
    private const double EPS = 10e-5;
    public enum InOut { INSIDE = 0, OUTSIDE = 1 }

    private ulong sign3(Vector3 point)
    {
        ulong res = 0;
        res |= (point.x < EPS) ? (ulong)4 : 0;
        res |= (point.x > -EPS) ? (ulong)32 : 0;
        res |= (point.y < EPS) ? (ulong)2 : 0;
        res |= (point.y > -EPS) ? (ulong)16 : 0;
        res |= (point.z < EPS) ? (ulong)1 : 0;
        res |= (point.z > -EPS) ? (ulong)8 : 0;
        return res;
    }

    //Determind which of the cube's faces is the point outside of 
    private ulong facePlane(Vector3 point)
    {
        ulong res = 0;
        if (point.x > 0.5f) res |= 0x01;
        if (point.x < -0.5f) res |= 0x02;
        if (point.y > 0.5f) res |= 0x04;
        if (point.y < -0.5f) res |= 0x08;
        if (point.z > 0.5f) res |= 0x10;
        if (point.z < -0.5f) res |= 0x20;
        return res;
    }

    //Determine which of the cube's 12 edges is the point outside of
    private ulong bevel2D(Vector3 point)
    {
        ulong res = 0;
        if (point.x + point.y > 1.0f) res |= 0x001;
        if (point.x - point.y > 1.0f) res |= 0x002;
        if (-point.x + point.y > 1.0f) res |= 0x004;
        if (-point.x - point.y > 1.0f) res |= 0x008;
        if (point.x + point.z > 1.0f) res |= 0x010;
        if (point.x - point.z > 1.0f) res |= 0x020;
        if (-point.x + point.z > 1.0f) res |= 0x040;
        if (-point.x - point.z > 1.0f) res |= 0x080;
        if (point.y + point.z > 1.0f) res |= 0x100;
        if (point.y - point.z > 1.0f) res |= 0x200;
        if (-point.y + point.z > 1.0f) res |= 0x400;
        if (-point.y - point.z > 1.0f) res |= 0x800;
        return res;
    }

    //Determine which of the cube's 8 faces the point is outside of
    private ulong bevel3D(Vector3 point)
    {
        ulong res = 0;
        if ((point.x + point.y + point.z) > 1.5f) res |= 0x01;
        if ((point.x + point.y - point.z) > 1.5f) res |= 0x02;
        if ((point.x - point.y + point.z) > 1.5f) res |= 0x04;
        if ((point.x - point.y - point.z) > 1.5f) res |= 0x08;
        if ((-point.x + point.y + point.z) > 1.5f) res |= 0x10;
        if ((-point.x + point.y - point.z) > 1.5f) res |= 0x20;
        if ((-point.x - point.y + point.z) > 1.5f) res |= 0x40;
        if ((-point.x - point.y - point.z) > 1.5f) res |= 0x80;
        return res;
    }

    //Test if the point on the tip of the
    //vector Co*P1P2 is on a face of the cube
    //consider faces only in the bitmask
    private ulong checkPoint(Vector3 p1, Vector3 p2, float co, ulong mask)
    {
        Vector3 planePoint;
        planePoint.x = Mathf.Lerp(p1.x, p2.x, co);
        planePoint.y = Mathf.Lerp(p1.y, p2.y, co);
        planePoint.z = Mathf.Lerp(p1.z, p2.z, co);
        return (facePlane(planePoint) & mask);
    }

    //Compute P1P2 intersection with face *planes*
    //then test the intersection point to see if it's on
    //the *cube* face
    //consider only face planes in mask
    //Note: zero bits in mask means face line is outside (weird note, anyway I think it means always outside)
    private ulong checkLine(Vector3 p1, Vector3 p2, ulong mask)
    {
        if ((0x01 & mask) != 0)
            if (checkPoint(p1, p2, (0.5f - p1.x) / (p2.x - p1.x), 0x3e) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        if ((0x02 & mask) != 0)
            if (checkPoint(p1, p2, (-0.5f - p1.x) / (p2.x - p1.x), 0x3d) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        if ((0x04 & mask) != 0)
            if (checkPoint(p1, p2, (0.5f - p1.y) / (p2.y - p1.y), 0x3b) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        if ((0x08 & mask) != 0)
            if (checkPoint(p1, p2, (-0.5f - p1.y) / (p2.y - p1.y), 0x37) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        if ((0x10 & mask) != 0)
            if (checkPoint(p1, p2, (0.5f - p1.z) / (p2.z - p1.z), 0x2f) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        if ((0x20 & mask) != 0)
            if (checkPoint(p1, p2, (-0.5f - p1.z) / (p2.z - p1.z), 0x1f) == (ulong)InOut.INSIDE) return (ulong)InOut.INSIDE;
        return (ulong)InOut.OUTSIDE;
    }

    //Test if a 3D point is INSIDE a 3D triangle
    private ulong pointTriIntersection(Vector3 p, Vector3[] t)
    {
        ulong sign12, sign23, sign31;
        Vector3 vect12, vect23, vect31, vect1h, vect2h, vect3h;
        Vector3 cross12_1p, cross23_2p, cross31_3p;

        //if p is outside the triangle AABB, then it's outside
        if (p.x > Mathf.Max(t[0].x, t[1].x, t[2].x)) return (ulong)InOut.OUTSIDE;
        if (p.y > Mathf.Max(t[0].y, t[1].y, t[2].y)) return (ulong)InOut.OUTSIDE;
        if (p.z > Mathf.Max(t[0].z, t[1].z, t[2].z)) return (ulong)InOut.OUTSIDE;
        if (p.x < Mathf.Min(t[0].x, t[1].x, t[2].x)) return (ulong)InOut.OUTSIDE;
        if (p.y < Mathf.Min(t[0].y, t[1].y, t[2].y)) return (ulong)InOut.OUTSIDE;
        if (p.z < Mathf.Min(t[0].z, t[1].z, t[2].z)) return (ulong)InOut.OUTSIDE;

        //For each triangle edge, create a vector equal to it, and a vector
        //from one corner and p
        //take the cross product of the two vectors, its X,Y,Z signs indicate
        //if p is inside or outside that side (or half space to be precise)
        vect12 = t[0] - t[1];
        vect1h = t[0] - p;
        cross12_1p = Vector3.Cross(vect12, vect1h);
        sign12 = sign3(cross12_1p);

        vect23 = t[1] - t[2];
        vect2h = t[1] - p;
        cross23_2p = Vector3.Cross(vect23, vect2h);
        sign23 = sign3(cross23_2p);

        vect31 = t[2] - t[0];
        vect3h = t[2] - p;
        cross31_3p = Vector3.Cross(vect31, vect3h);
        sign31 = sign3(cross31_3p);

        //if all the products agree in their signs
        //then the point must be inside all three sides
        //since it CANNOT be outside all sides at the same time
        return ((sign12 & sign23 & sign31) == 0) ? (ulong)InOut.OUTSIDE : (ulong)InOut.INSIDE;
    }

    //Main algorithm execution body
    //a triangle is compared with a UNIT cube
    //CENTERED ON THE ORIGIN
    //returns INSIDE or OUTSIDE
    //if the triangle intersect or not
    public ulong triCubeIntersection(Vector3[] t){
        ulong v1Test,v2Test,v3Test;
        float d,denom;
        Vector3 vect12,vect13,norm;
        Vector3 hitpp,hitpn,hitnp,hitnn;

        //First check if any of the tri vertices is inside the cube
        if((v1Test=facePlane(t[0]))==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        if((v2Test=facePlane(t[1]))==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        if((v3Test=facePlane(t[2]))==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;

        //If ALL vertices were outside one or more of the cube face-planes
        //return an immediate rejection
        if((v1Test&v2Test&v3Test)!=0)return (ulong) InOut.OUTSIDE;

        //Do the same test for the 12 edge-planes
        v1Test|=bevel2D(t[0])<<8;
        v2Test|=bevel2D(t[1])<<8;
        v3Test|=bevel2D(t[2])<<8;
        if((v1Test&v2Test&v3Test)!=0) return (ulong)InOut.OUTSIDE;

        //Do the same test for the 8 corner-planes
        v1Test|=bevel3D(t[0])<<24;
        v2Test|=bevel3D(t[1])<<24;
        v3Test|=bevel3D(t[2])<<24;
        if((v1Test&v2Test&v3Test)!=0)return (ulong)InOut.OUTSIDE;

        //If vertices 1 & 2 as a pair, cannot be rejected by the above tests
        //then see if the edge v1v2 intersects the cube, do the same for
        //all the triangle edges
        //Pass to the intersection algor only the results of the tests
        //so that only the cube faces that are spanned by each edge are tested
        if((v1Test&v2Test)==0)
            if(checkLine(t[0],t[1],v1Test|v2Test)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        if((v1Test&v3Test)==0)
            if(checkLine(t[0],t[2],v1Test|v3Test)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        if((v2Test & v3Test)==0)
            if (checkLine(t[1],t[2],v2Test|v3Test)==(ulong)InOut.INSIDE)return(ulong)InOut.INSIDE;

        //By now we know the triangle exterior (edges and vertices)
        //is not intersecting the cube
        //we test if the cube is intersecting the interior of the triangle
        //we do this by looking for intersection between the cube diagonals
        //and the triangle, first we find the intersection of the four diagonals
        //with the plane of the triangle, then check if the intersectin is 
        //INSIDE the cube, and then check if the intersection point is INSIDE
        //the triangle itself

        //first we calculate the normal of the triangle
        vect12=t[0]-t[1];
        vect13=t[0]-t[2];
        norm=Vector3.Cross(vect12,vect13);

        /* The normal vector "norm" X,Y,Z components are the coefficients */
        /* of the triangles AX + BY + CZ + D = 0 plane equation.  If we   */
        /* solve the plane equation for X=Y=Z (a diagonal), we get        */
        /* -D/(A+B+C) as a metric of the distance from cube center to the */
        /* diagonal/plane intersection.  If this is between -0.5 and 0.5, */
        /* the intersection is inside the cube.  If so, we continue by    */
        /* doing a point/triangle intersection.                           */
        /* Do this for all four diagonals.                                */

        d=norm.x*t[0].x+norm.y*t[0].y+norm.z*t[0].z;

        //If one of the diagonals is parallel to the plane, the other will intersect the plane
        if(Mathf.Abs(denom=(norm.x+norm.y+norm.z))>EPS)
        //Skip parallel diagonals to the plane; a division by 0 can occur
        {
            hitpp.x=hitpp.y=hitpp.z=d/denom;
            if(Mathf.Abs(hitpp.x)<=0.5f){
                if(pointTriIntersection(hitpp,t)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
            }
        }
        if(Mathf.Abs(denom=(norm.x+norm.y-norm.z))>EPS){
            hitpn.z=-(hitpn.x=hitpn.y=d/denom);
            if(Mathf.Abs(hitpn.x)<=0.5f)
                if(pointTriIntersection(hitpn,t)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        }
        if(Mathf.Abs(denom=(norm.x-norm.y+norm.z))>EPS){
            hitnp.y=(hitnp.x=hitnp.z=d/denom);
            if(Mathf.Abs(hitnp.x)<=0.5f)
                if(pointTriIntersection(hitnp,t)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        }
        if((denom=(norm.x-norm.y-norm.z))>EPS){ //Why no ABS?
            hitnn.y=hitnn.z=-(hitnn.x=d/denom);
            if(Mathf.Abs(hitnn.x)<=0.5f)
                if(pointTriIntersection(hitnn,t)==(ulong)InOut.INSIDE)return (ulong)InOut.INSIDE;
        }

        //Finished tests

        return (ulong)InOut.OUTSIDE;
    }

    public ulong triCubeIntersection(Vector3[] t,Transform cube){
        for(int i=0;i<3;i++) t[i]=cube.InverseTransformPoint(t[i]);
        return triCubeIntersection(t);
    }
}
