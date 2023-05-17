using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class FrictionSeqImpSolver : Solver
{
    [SerializeField]
    private int iterations = 50; //Higher gives better accuracy but needs more cpu power

    const float biasFactor = 0.001f;
    const float depthThreshold = 0.01f;


    public override void resolveCullision(CullisionInfo[] cullisions)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            for (int i = 0; i < cullisions.Length; i++)
            {
                for (int j = 0; j < cullisions[i].contactPointsA.Length; j++)
                {
                    if (!cullisions[i].cullided) continue;
                    if (!cullisions[i].hasContactPointA || !cullisions[i].hasContactPointB)
                    {
                        Debug.Log("A collision was passed with no contact points, only linear velocity was used to resolve");
                        Debug.Log(cullisions[i]);
                    }

                    float bias = 0;
                    if (math.abs(cullisions[i].depth) > depthThreshold)
                        bias = -biasFactor * math.abs(cullisions[i].depth) / Time.fixedDeltaTime;

                    Vector3 normal = cullisions[i].normal.normalized;
                    Vector3 t1 = cullisions[i].t1;
                    Vector3 t2 = cullisions[i].t2;
                    Vector3 rA = cullisions[i].contactPointsA[j] - cullisions[i].first.center();
                    Vector3 rB = cullisions[i].contactPointsB[j] - cullisions[i].second.center();

                    float12 jacobian = new float12(-normal, Vector3.Cross(-rA,normal)
                                                , normal, Vector3.Cross(rB,normal));

                    float12x12 inverseMass = new float12x12(1.0f / cullisions[i].first.getRigidbody().mass,
                                                    1.0f / Shape.inertiaScalar(cullisions[i].first.getTensorInertia(), (Vector3.Cross(-rA,normal))),
                                                    1.0f / cullisions[i].second.getRigidbody().mass,
                                                    1.0f / Shape.inertiaScalar(cullisions[i].second.getTensorInertia(), (Vector3.Cross(rB,normal))));

                    float12 velocities = new float12(cullisions[i].first.getRigidbodyDriver().velocity,
                                                    cullisions[i].first.getRigidbodyDriver().getAngularVelocity(),
                                                    cullisions[i].second.getRigidbodyDriver().velocity,
                                                    cullisions[i].second.getRigidbodyDriver().getAngularVelocity());

                    float lambda = -(float12x12.rowColMult(jacobian, velocities) + bias) / (float12x12.rowColMult((jacobian * inverseMass), jacobian));

                    float normalImpulseSumCopy = cullisions[i].normalImpulseSum;
                    cullisions[i].normalImpulseSum += lambda;
                    cullisions[i].normalImpulseSum = math.clamp(cullisions[i].normalImpulseSum, 0, float.MaxValue);
                    lambda = cullisions[i].normalImpulseSum - normalImpulseSumCopy;

                    float12 deltaV = inverseMass * jacobian * lambda;

                    cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[0], deltaV.floats[1], deltaV.floats[2]));
                    cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[3], deltaV.floats[4], deltaV.floats[5]));
                    cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[6], deltaV.floats[7], deltaV.floats[8]));
                    cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[9], deltaV.floats[10], deltaV.floats[11]));

                    // velocities = new float12(cullisions[i].first.getRigidbodyDriver().velocity,
                    //                             cullisions[i].first.getRigidbodyDriver().getAngularVelocity(),
                    //                             cullisions[i].second.getRigidbodyDriver().velocity,
                    //                             cullisions[i].second.getRigidbodyDriver().getAngularVelocity());

                    // float12 jacobianT1 = new float12(-t1, Vector3.Cross(t1, rA),
                    //                         t1, Vector3.Cross(-t1, rB));

                    // float12 jacobianT2 = new float12(-t2, Vector3.Cross(t2, rA),
                    //                         t2, Vector3.Cross(-t2, rB));

                    // float lambdaT1 = -(float12x12.rowColMult(jacobianT1, velocities)) / (float12x12.rowColMult((jacobianT1 * inverseMass), jacobianT1));

                    // float lambdaT2 = -(float12x12.rowColMult(jacobianT2, velocities)) / (float12x12.rowColMult((jacobianT2 * inverseMass), jacobianT2));

                    // float tangentImpulseSum1Copy = cullisions[i].tangentImpulseSum1;
                    // float tangentImpulseSum2Copy = cullisions[i].tangentImpulseSum2;

                    // cullisions[i].tangentImpulseSum1 += lambdaT1 + lambdaT2;
                    // cullisions[i].tangentImpulseSum2 += lambdaT1 + lambdaT2;

                    // float cf = 1.0f;

                    // cullisions[i].tangentImpulseSum1 = math.clamp(cullisions[i].tangentImpulseSum1,
                    //                                              -cf * cullisions[i].normalImpulseSum,
                    //                                              cf * cullisions[i].normalImpulseSum);

                    // cullisions[i].tangentImpulseSum2 = math.clamp(cullisions[i].tangentImpulseSum2,
                    //                                                                  -cf * cullisions[i].normalImpulseSum,
                    //                                                                  cf * cullisions[i].normalImpulseSum);

                    // lambdaT1 = cullisions[i].tangentImpulseSum1 - tangentImpulseSum1Copy;
                    // lambdaT2 = cullisions[i].tangentImpulseSum2 - tangentImpulseSum2Copy;

                    
                    // float12 deltaVT1 = inverseMass * jacobianT1 * lambdaT1;
                    // float12 deltaVT2 = inverseMass * jacobianT2 * lambdaT2;

                    // cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT1.floats[0], deltaVT1.floats[1], deltaVT1.floats[2]));
                    // cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT1.floats[3], deltaVT1.floats[4], deltaVT1.floats[5]));
                    // cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT1.floats[6], deltaVT1.floats[7], deltaVT1.floats[8]));
                    // cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT1.floats[9], deltaVT1.floats[10], deltaVT1.floats[11]));

                    // cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT2.floats[0], deltaVT2.floats[1], deltaVT2.floats[2]));
                    // cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT2.floats[3], deltaVT2.floats[4], deltaVT2.floats[5]));
                    // cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT2.floats[6], deltaVT2.floats[7], deltaVT2.floats[8]));
                    // cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT2.floats[9], deltaVT2.floats[10], deltaVT2.floats[11]));
                }
            }
        }
    }
}
