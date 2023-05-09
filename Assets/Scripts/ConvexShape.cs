using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvexShape
{
    // list of vertices
    private List<Vector3> vertices;

    // constructor
    public ConvexShape(List<Vector3> vertices)
    {
        this.vertices = vertices;
    }

    // support function that returns the farthest point in a direction
    public Vector3 Support(Vector3 direction)
    {
        // initialize the farthest point and the maximum dot product
        Vector3 farthest = vertices[0];
        double maxDot = Vector3.Dot(farthest, direction);

        // loop over the remaining vertices
        for (int i = 1; i < vertices.Count; i++)
        {
            // compute the dot product of the current vertex and the direction
            double dot = Vector3.Dot(vertices[i], direction);

            // if the dot product is higher, update the farthest point and the maximum dot product
            if (dot > maxDot)
            {
                farthest = vertices[i];
                maxDot = dot;
            }
        }

        // return the farthest point
        return farthest;
    }
    // define a function to compute the Minkowski difference of two shapes
    public static Vector3 MinkowskiDifference(ConvexShape shapeA, ConvexShape shapeB, Vector3 direction)
    {
        // get the farthest point of shape A along the direction
        Vector3 pointA = shapeA.Support(direction);

        // get the farthest point of shape B along the opposite direction
        Vector3 pointB = shapeB.Support(-direction);

        // return the difference of the two points
        return pointA - pointB;
    }

    // define a function to update the simplex and the direction for a line case
    public static bool UpdateLine(List<Vector3> simplex, ref Vector3 direction)
    {
        // get the last and second last points of the simplex
        Vector3 A = simplex[1];
        Vector3 B = simplex[0];

        // get the vectors from A to B and from A to origin
        Vector3 AB = B - A;
        Vector3 AO = -A;

        // check if the origin is in the same direction as AB
        if (Vector3.Dot(AB, AO) > 0)
        {
            // update the direction to be perpendicular to AB towards origin
            direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);
        }
        else
        {
            // remove B from simplex and update direction to AO
            simplex.RemoveAt(0);
            direction = AO;
        }

        // return false as termination condition is not met yet
        return false;
    }

    // define a function to update the simplex and the direction for a triangle case
    public static bool UpdateTriangle(List<Vector3> simplex, ref Vector3 direction)
    {
        // get the last, second last and third last points of the simplex
        Vector3 A = simplex[2];
        Vector3 B = simplex[1];
        Vector3 C = simplex[0];

        // get the vectors from A to B, C and origin
        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 AO = -A;

        // get the normals of the two triangle edges
        Vector3 ABC = Vector3.Cross(AB, AC);
        Vector3 ABX = Vector3.Cross(AB, ABC);

        // check if the origin is in region AB
        if (Vector3.Dot(ABX, AO) > 0)
        {
            // remove C from simplex and update direction to be perpendicular to AB towards origin
            simplex.RemoveAt(0);
            direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);
            return false;
        }

        // get the normal of the other triangle edge
        Vector3 ACX = Vector3.Cross(ABC, AC);

        // check if the origin is in region AC
        if (Vector3.Dot(ACX, AO) > 0)
        {
            // remove B from simplex and update direction to be perpendicular to AC towards origin
            simplex.RemoveAt(1);
            direction = Vector3.Cross(Vector3.Cross(AC, AO), AC);
            return false;
        }

        // check if the origin is above or below the triangle
        if (Vector3.Dot(ABC, AO) > 0)
        {
            // keep the simplex and update direction to be perpendicular to ABC towards origin
            direction = ABC;
            return false;
        }
        else if (Vector3.Dot(ABC, AO) < 0)
        {
            // swap B and C in simplex and update direction to be perpendicular to ABC away from origin
            simplex[0] = B;
            simplex[1] = C;
            direction = -ABC;
            return false;
        }
        else
        {
            // origin is on the triangle, collision detected
            return true;
        }
    }
    // define a function to update the simplex and the direction for a tetrahedron case
    public static bool UpdateTetrahedron(List<Vector3> simplex, ref Vector3 direction)
    {
        // get the last, second last, third last and fourth last points of the simplex
        Vector3 A = simplex[3];
        Vector3 B = simplex[2];
        Vector3 C = simplex[1];
        Vector3 D = simplex[0];

        // get the vectors from A to B, C, D and origin
        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 AD = D - A;
        Vector3 AO = -A;

        // get the normals of the three faces of the tetrahedron
        Vector3 ABC = Vector3.Cross(AB, AC);
        Vector3 ACD = Vector3.Cross(AC, AD);
        Vector3 ADB = Vector3.Cross(AD, AB);

        // check if the origin is in region ABC
        if (Vector3.Dot(ABC, AO) > 0)
        {
            // remove D from simplex and update direction to be perpendicular to ABC towards origin
            simplex.RemoveAt(0);
            direction = ABC;
            return false;
        }

        // check if the origin is in region ACD
        if (Vector3.Dot(ACD, AO) > 0)
        {
            // remove B from simplex and update direction to be perpendicular to ACD towards origin
            simplex.RemoveAt(2);
            direction = ACD;
            return false;
        }

        // check if the origin is in region ADB
        if (Vector3.Dot(ADB, AO) > 0)
        {
            // remove C from simplex and update direction to be perpendicular to ADB towards origin
            simplex.RemoveAt(1);
            direction = ADB;
            return false;
        }

        // origin is inside the tetrahedron, collision detected
        return true;
    }
}
