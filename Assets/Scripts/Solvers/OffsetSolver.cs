using UnityEngine;

public class OffsetSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        A.GetComponent<RigidbodyDriver>().applyLinearMomentum(B.GetComponent<RigidbodyDriver>());
        //apply angular momentum

        float depthA = cullision.depth;
        float depthB = cullision.depth;

        if (cullision.hasContactPointA)
        {
            Vector3 correctPoint = cullision.contactPointA + cullision.normal.normalized * cullision.depth / 2.0f;//Split the depth linear and angular
            Vector3 currentPos = cullision.contactPointA - A.transform.position;
            Vector3 correctPos = correctPoint - A.transform.position;

            Vector3 normal = Vector3.Cross(currentPos, correctPos);

            if (normal != Vector3.zero)
            {
                depthA /= 2.0f;
                A.transform.Rotate(normal, Vector3.Angle(currentPos, correctPos), Space.Self);
            }
        }

        if (cullision.hasContactPointB)
        {
            Vector3 correctPoint = cullision.contactPointB - cullision.normal.normalized * cullision.depth / 2.0f;
            Vector3 currentPos = cullision.contactPointB - B.transform.position;
            Vector3 correctPos = correctPoint - B.transform.position;

            Vector3 normal = Vector3.Cross(currentPos, correctPos);

            if (normal != Vector3.zero)
            {
                depthB /= 2.0f;
                B.transform.Rotate(Vector3.Cross(currentPos, correctPos), Vector3.Angle(currentPos, correctPos), Space.Self);
            }
        }

        float AmassFactor = A.mass / (A.mass + B.mass);
        float BmassFactor = B.mass / (A.mass + B.mass);

        A.transform.position += BmassFactor * cullision.normal.normalized * depthA;
        B.transform.position += -AmassFactor * cullision.normal.normalized * depthB;

    }
}