using UnityEngine;
using Unity.Mathematics;
public class SequentialImpulseSolver : Solver
{
    private int iterations = 100;

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
                        bias = biasFactor * cullisions[i].depth / Time.fixedDeltaTime;

                    Vector3 normal = cullisions[i].normal.normalized * cullisions[i].depth;
                    Vector3 rA = cullisions[i].contactPointsA[j] - cullisions[i].first.center();
                    Vector3 rB = cullisions[i].contactPointsB[j] - cullisions[i].second.center();

                    float12 jacobian = new float12(-normal, Vector3.Cross(normal, rA)
                                                , normal, Vector3.Cross(-normal, rB));

                    float12x12 inverseMass = new float12x12(1.0f / cullisions[i].first.getRigidbody().mass,
                                                    1.0f / Shape.inertiaScalar(cullisions[i].first.getTensorInertia(), (Vector3.Cross(normal, rA))),
                                                    1.0f / cullisions[i].second.getRigidbody().mass,
                                                    1.0f / Shape.inertiaScalar(cullisions[i].second.getTensorInertia(), (Vector3.Cross(-normal, rB))));

                    float12 velocities = new float12(cullisions[i].first.getRigidbodyDriver().velocity,
                                                    cullisions[i].first.getRigidbodyDriver().getAngularVelocity(),
                                                    cullisions[i].second.getRigidbodyDriver().velocity,
                                                    cullisions[i].second.getRigidbodyDriver().getAngularVelocity());

                    float lambda = -(float12x12.rowColMult(jacobian, velocities) + bias) / (float12x12.rowColMult((jacobian * inverseMass), jacobian));

                    float normalImpulseSumCopy = cullisions[i].normalImpulseSum;
                    cullisions[i].normalImpulseSum += lambda;
                    cullisions[i].normalImpulseSum = math.clamp(cullisions[i].normalImpulseSum, float.MinValue, 0);
                    lambda = cullisions[i].normalImpulseSum - normalImpulseSumCopy;

                    float12 deltaV = inverseMass * jacobian * lambda;

                    cullisions[i].first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[0], deltaV.floats[1], deltaV.floats[2]));
                    cullisions[i].first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[3], deltaV.floats[4], deltaV.floats[5]));
                    cullisions[i].second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[6], deltaV.floats[7], deltaV.floats[8]));
                    cullisions[i].second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[9], deltaV.floats[10], deltaV.floats[11]));
                }
            }
        }
    }
}