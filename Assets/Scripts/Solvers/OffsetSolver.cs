using UnityEngine;

public class OffsetSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        A.GetComponent<RigidbodyDriver>().applyLinearMomentum(B.GetComponent<RigidbodyDriver>());
        A.GetComponent<RigidbodyDriver>().applyAngularMomentum(B.GetComponent<RigidbodyDriver>());

        float depthA = cullision.depth;
        float depthB = cullision.depth;
        
        //TODO take tensor value into account when adding angular velocity
        if (cullision.hasContactPointA)
        {
            Vector3 correctPoint = cullision.contactPointA + cullision.normal.normalized * cullision.depth / 2.0f;//Split the depth linear and angular
            Vector3 currentPos = cullision.contactPointA - A.transform.position;
            Vector3 correctPos = correctPoint - A.transform.position;

            Vector3 normal = Vector3.Cross(currentPos, correctPos);

            if (normal != Vector3.zero)
            {
                depthA /= 2.0f;
                //A.transform.Rotate(normal, Vector3.Angle(currentPos, correctPos), Space.Self);
                A.GetComponent<RigidbodyDriver>().addAngularVelocity(normal*Vector3.Angle(currentPos,correctPos)/Time.fixedDeltaTime);
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
                //B.transform.Rotate(Vector3.Cross(currentPos, correctPos), Vector3.Angle(currentPos, correctPos), Space.Self);
                B.GetComponent<RigidbodyDriver>().addAngularVelocity(normal*Vector3.Angle(currentPos,correctPos)/Time.fixedDeltaTime);
            }
        }

        float AmassFactor = A.mass / (A.mass + B.mass);
        float BmassFactor = B.mass / (A.mass + B.mass);

        //A.GetComponent<RigidbodyDriver>().addLinearVelocity(BmassFactor*cullision.normal.normalized*depthA*Time.fixedDeltaTime);
        //B.GetComponent<RigidbodyDriver>().addLinearVelocity(-AmassFactor*cullision.normal.normalized*depthB*Time.fixedDeltaTime);

        A.transform.position += BmassFactor * cullision.normal.normalized * depthA;
        B.transform.position += -AmassFactor * cullision.normal.normalized * depthB;

    }
}