using Unity.Mathematics;
using UnityEngine;

//Commented out to reduce maintainance
/*public class OffsetSolver //Doesn't implement Solver interface any longer
{
    public void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if (!cullision.cullided) return;
        //A.GetComponent<RigidbodyDriver>().applyLinearMomentum(B.GetComponent<RigidbodyDriver>());
        //A.GetComponent<RigidbodyDriver>().applyAngularMomentum(B.GetComponent<RigidbodyDriver>());
        float depthA,depthB;
        applyAngularOffset(cullision,A,B,out depthA,out depthB);
        //applyLinearOffset(cullision,A,B,depthA/1000.0f,depthB/1000.0f);
    }

    private void applyAngularOffset(CullisionInfo cullision, Rigidbody A, Rigidbody B, out float depthA, out float depthB)
    {
        depthA = cullision.depth*100; //FIXME just for testing
        depthB = cullision.depth*100;
        if (cullision.hasContactPointA || cullision.hasContactPointB)
        {
            float3 normal = Vector3.zero;
            float inertiaScalarA, inertiaScalarB;
            float angleA = .0f, angleB = .0f;

            if (cullision.hasContactPointA)
            {
                Vector3 correctPoint = cullision.contactPointA + cullision.normal.normalized * cullision.depth / 2.0f;//Split the depth linear and angular
                Vector3 currentPos = cullision.contactPointA - A.transform.position;
                Vector3 correctPos = correctPoint - A.transform.position;

                normal = Vector3.Cross(currentPos, correctPos);
                angleA = Vector3.Angle(currentPos, correctPos);
            }
            if (cullision.hasContactPointB)
            {
                Vector3 correctPoint = cullision.contactPointB - cullision.normal.normalized * cullision.depth / 2.0f;
                Vector3 currentPos = cullision.contactPointB - B.transform.position;
                Vector3 correctPos = correctPoint - B.transform.position;

                angleB = Vector3.Angle(currentPos, correctPos);
                if (math.all(normal == float3.zero))
                    normal = -Vector3.Cross(currentPos, correctPos);
            }

            if (math.any(normal != float3.zero))
            {
                normal = math.normalize(normal);
                inertiaScalarA = math.mul(normal, math.mul(A.GetComponent<RigidbodyDriver>().getInertiaTensor(), normal));
                inertiaScalarB = math.mul(-normal, math.mul(B.GetComponent<RigidbodyDriver>().getInertiaTensor(), -normal));

                float AInertiaFactor = inertiaScalarA / (inertiaScalarA + inertiaScalarB);
                float BInertiaFactor = inertiaScalarB / (inertiaScalarA + inertiaScalarB);

                A.transform.Rotate(normal, angleA * BInertiaFactor, Space.Self);
                //A.GetComponent<RigidbodyDriver>().addAngularVelocity(BInertiaFactor*normal * angleA * Time.fixedDeltaTime);

                B.transform.Rotate(-normal, angleB * AInertiaFactor, Space.Self);
                //B.GetComponent<RigidbodyDriver>().addAngularVelocity(-AInertiaFactor*normal * angleB * Time.fixedDeltaTime);
            }

            if (angleA != 0) depthA /= 2.0f;
            if (angleB != 0) depthB /= 2.0f;
        }
    }

    private void applyLinearOffset(CullisionInfo cullision, Rigidbody A, Rigidbody B, float depthA, float depthB)
    {
        float AmassFactor = A.mass / (A.mass + B.mass);
        float BmassFactor = B.mass / (A.mass + B.mass);

        //A.GetComponent<RigidbodyDriver>().addLinearVelocity(BmassFactor*cullision.normal.normalized*depthA*Time.fixedDeltaTime);
        //B.GetComponent<RigidbodyDriver>().addLinearVelocity(-AmassFactor*cullision.normal.normalized*depthB*Time.fixedDeltaTime);

        A.transform.position += BmassFactor * cullision.normal.normalized * depthA;
        B.transform.position += -AmassFactor * cullision.normal.normalized * depthB;
    }
}*/