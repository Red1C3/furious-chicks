using UnityEngine;
using Unity.Mathematics;

public class ImpulseSolver : Solver
{
    public override void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if (!cullision.cullided) return;
        Vector3 normal = cullision.normal.normalized * cullision.depth;
        Vector3 rA = cullision.contactPointA - cullision.first.center();
        Vector3 rB = cullision.contactPointB - cullision.second.center();

        float12 jacobian = new float12(-normal, Vector3.Cross(normal, rA)
                                    , normal, Vector3.Cross(-normal, rB));

        float12x12 inverseMass = new float12x12(1.0f / cullision.first.getRigidbody().mass,
                                        1.0f / Shape.inertiaScalar(cullision.first.getTensorInertia(), (Vector3.Cross(normal, rA))),
                                        1.0f / cullision.second.getRigidbody().mass,
                                        1.0f / Shape.inertiaScalar(cullision.second.getTensorInertia(), (Vector3.Cross(-normal, rB))));

        float12 velocities = new float12(cullision.first.getRigidbodyDriver().velocity,
                                        cullision.first.getRigidbodyDriver().getAngularVelocity(),
                                        cullision.second.getRigidbodyDriver().velocity,
                                        cullision.second.getRigidbodyDriver().getAngularVelocity());

        float lambda = -(float12x12.rowColMult(jacobian, velocities)) / (float12x12.rowColMult((jacobian * inverseMass), jacobian));
        lambda = math.clamp(lambda, float.MinValue, 0);

        float12 deltaV = inverseMass * jacobian * lambda;

        cullision.first.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[0], deltaV.floats[1], deltaV.floats[2]));
        cullision.first.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[3], deltaV.floats[4], deltaV.floats[5]));
        cullision.second.getRigidbodyDriver().addLinearVelocity(new Vector3(deltaV.floats[6], deltaV.floats[7], deltaV.floats[8]));
        cullision.second.getRigidbodyDriver().addAngularVelocity(new Vector3(deltaV.floats[9], deltaV.floats[10], deltaV.floats[11]));
    }
}