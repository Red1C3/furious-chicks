using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class FrictionSeqImpSolver : Solver
{
    [SerializeField]
    private int iterations = 50; //Higher gives better accuracy but needs more cpu power

    private const float biasFactor = 0.15f;
    private const float depthThreshold = 0.01f;
    private static float12 jacobian = new float12(0, 0, 0, 0), jacobianT1 = new float12(0, 0, 0, 0), jacobianT2 = new float12(0, 0, 0, 0);
    private static float12 velocities = new float12(0, 0, 0, 0);
    private static float12x12 inverseMass = new float12x12(0, 0, 0, 0);
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


                    Vector3 normal = cullisions[i].normal.normalized;
                    Vector3 t1 = cullisions[i].t1;
                    Vector3 t2 = cullisions[i].t2;
                    Vector3 rA = cullisions[i].contactPointsA[j] - cullisions[i].first.center();
                    Vector3 rB = cullisions[i].contactPointsB[j] - cullisions[i].second.center();

                    jacobian.set(-normal, Vector3.Cross(-rA, normal)
                                , normal, Vector3.Cross(rB, normal));

                    inverseMass.set(cullisions[i].first.getRigidbodyDriver().getInverseMassVector3(),
                                    cullisions[i].first.getRigidbodyDriver().getInverseInertiaVector3((Vector3.Cross(-rA, normal))),
                                    cullisions[i].second.getRigidbodyDriver().getInverseMassVector3(),
                                    cullisions[i].second.getRigidbodyDriver().getInverseInertiaVector3((Vector3.Cross(rB, normal))));

                    velocities.set(cullisions[i].first.getRigidbodyDriver().velocity,
                                    cullisions[i].first.getRigidbodyDriver().getAngularVelocity(),
                                    cullisions[i].second.getRigidbodyDriver().velocity,
                                    cullisions[i].second.getRigidbodyDriver().getAngularVelocity());


                    float bias = 0;
                    float cR = (cullisions[i].first.getBouncinessCo() + cullisions[i].second.getBouncinessCo()) / 2.0f;
                    Vector3 wRA = Vector3.Cross(cullisions[i].first.getRigidbodyDriver().getAngularVelocity(), rA);
                    Vector3 wRB = Vector3.Cross(cullisions[i].second.getRigidbodyDriver().getAngularVelocity(), rB);
                    Vector3 vA = cullisions[i].first.getRigidbodyDriver().velocity;
                    Vector3 vB = cullisions[i].second.getRigidbodyDriver().velocity;
                    float bouncinessTerm = Vector3.Dot(-vA - wRA + vB + wRB, normal);
                    bouncinessTerm *= cR;
                    if (bouncinessTerm > 0.1f || bouncinessTerm < -0.1f)
                        bias = -bouncinessTerm;
                    if (math.abs(cullisions[i].depth) > depthThreshold)
                    {

                        bias += (-biasFactor * math.abs(cullisions[i].depth) / Time.fixedDeltaTime);
                    }


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

                    jacobianT1.set(-t1, Vector3.Cross(-rA, t1),
                                    t1, Vector3.Cross(rB, t1));

                    jacobianT2.set(-t2, Vector3.Cross(-rA, t2),
                                    t2, Vector3.Cross(rB, t2));

                    float lambdaT1 = -(float12x12.rowColMult(jacobianT1, velocities)) / (float12x12.rowColMult((jacobianT1 * inverseMass), jacobianT1));

                    float lambdaT2 = -(float12x12.rowColMult(jacobianT2, velocities)) / (float12x12.rowColMult((jacobianT2 * inverseMass), jacobianT2));

                    float tangentImpulseSum1Copy = cullisions[i].tangentImpulseSum1;
                    float tangentImpulseSum2Copy = cullisions[i].tangentImpulseSum2;

                    cullisions[i].tangentImpulseSum1 += lambdaT1;
                    cullisions[i].tangentImpulseSum2 += lambdaT2;

                    float cf = (cullisions[i].first.getFrictionCo() + cullisions[i].second.getFrictionCo()) / 2.0f;

                    cullisions[i].tangentImpulseSum1 = math.clamp(cullisions[i].tangentImpulseSum1,
                                                                 -cf * cullisions[i].normalImpulseSum,
                                                                 cf * cullisions[i].normalImpulseSum + math.EPSILON);

                    cullisions[i].tangentImpulseSum2 = math.clamp(cullisions[i].tangentImpulseSum2,
                                                                                     -cf * cullisions[i].normalImpulseSum,
                                                                                     cf * cullisions[i].normalImpulseSum + math.EPSILON);

                    lambdaT1 = cullisions[i].tangentImpulseSum1 - tangentImpulseSum1Copy;
                    lambdaT2 = cullisions[i].tangentImpulseSum2 - tangentImpulseSum2Copy;


                    float12 deltaVT1 = inverseMass * jacobianT1 * lambdaT1;

                    cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT1.floats[0], deltaVT1.floats[1], deltaVT1.floats[2]));
                    cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT1.floats[3], deltaVT1.floats[4], deltaVT1.floats[5]));
                    cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT1.floats[6], deltaVT1.floats[7], deltaVT1.floats[8]));
                    cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT1.floats[9], deltaVT1.floats[10], deltaVT1.floats[11]));

                    float12 deltaVT2 = inverseMass * jacobianT2 * lambdaT2;
                    cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT2.floats[0], deltaVT2.floats[1], deltaVT2.floats[2]));
                    cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT2.floats[3], deltaVT2.floats[4], deltaVT2.floats[5]));
                    cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaVT2.floats[6], deltaVT2.floats[7], deltaVT2.floats[8]));
                    cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaVT2.floats[9], deltaVT2.floats[10], deltaVT2.floats[11]));
                }
            }
        }
    }
}
