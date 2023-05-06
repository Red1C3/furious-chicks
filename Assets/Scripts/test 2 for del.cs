using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2fordel : MonoBehaviour
{
    public static double GJK(ConvexShape shapeA, ConvexShape shapeB)
    {
        // initialize the simplex list and the direction vector
        List<Vector3> simplex = new List<Vector3>();
        Vector3 direction = new Vector3(1, 0, 0);

        // add an initial point to the simplex
        simplex.Add(ConvexShape.MinkowskiDifference(shapeA, shapeB, direction));

        // negate the direction
        direction = -direction;

        // loop until collision or convergence
        while (true)
        {
            // add a new point to the simplex
            simplex.Add(ConvexShape.MinkowskiDifference(shapeA, shapeB, direction));

            // check if the new point is past the origin
            if (Vector3.Dot(simplex.Last(), direction) < 0)
            {
                // origin is not in the Minkowski difference, no collision
                return -1;
            }

            // update the simplex and the direction depending on the size of the simplex
            switch (simplex.Count)
            {
                case 2:
                    // line case
                    if (ConvexShape.UpdateLine(simplex, ref direction))
                        return 0; // collision detected
                    break;
                case 3:
                    // triangle case
                    if (ConvexShape.UpdateTriangle(simplex, ref direction))
                        return 0; // collision detected
                    break;
                case 4:
                    // tetrahedron case
                    if (ConvexShape.UpdateTetrahedron(simplex, ref direction))
                        return 0; // collision detected
                    break;
            }

            // check for convergence
            if (Vector3.Magnitude(simplex.Last()) < 1e-6)
            {
                // origin is close enough to the simplex, return distance
                return Vector3.Magnitude(simplex.Last());
            }
        }
    }

    public GameObject go1,go2;
    ConvexShape createConvexShape(GameObject go){
        List<Vector3> vertices = (go.GetComponent<MeshFilter>().mesh.vertices).ToList();
        for (int i=0;i<vertices.Count;i++)
        {
            vertices[i]=go.transform.TransformPoint(vertices[i]);
        }
        ConvexShape cs = new ConvexShape(vertices);
        return cs;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ConvexShape cs1=createConvexShape(go1),cs2=createConvexShape(go2);
        // call the gjk function with the two convex shape objects and store the return value
        double distance = GJK(cs1, cs2);

        // print the result
        Debug.Log("Distance between cubes :"+distance);
    }
}
