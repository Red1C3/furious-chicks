using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public float force;
    private float angleX;
    private float angleY;
    private float angleZ;
    public LineRenderer lineRenderer, parabolaRenderer;
    public float cameraAway = 3;

    public bool fired { get; private set; }
    private bool press = false, cameraNotSetOnStart = false;

    private RigidbodyDriver rigidbodyDriver;
    private int maxSamples;
    private float[] samplesX, samplesY;
    void Start()
    {
        fired = false;
        transform.position = new Vector3(0, 0, -force);
        if (LevelCtrlr.playerView)
            LevelCtrlr.cam.transform.position = new Vector3(0, 0, -(force + cameraAway));
        else
            cameraNotSetOnStart = true;

        rigidbodyDriver = GetComponent<RigidbodyDriver>();
        rigidbodyDriver.useGravity = false;

        lineRenderer.SetPosition(0, new Vector3(-1, 0, 0));
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.SetPosition(2, new Vector3(1, 0, 0));
        maxSamples = Mathf.FloorToInt(Engine.ground.transform.localScale.z * 2 + Mathf.Abs(transform.transform.position.z));
        samplesX = new float[maxSamples];
        samplesY = new float[maxSamples];
        parabolaRenderer.positionCount = maxSamples;
        for (int i = 0; i < maxSamples; i++)
        {
            samplesX[i] = i;
        }
    }

    void FixedUpdate()
    {
        if (LevelCtrlr.playerView && !fired)
        {
            if (cameraNotSetOnStart)
            {
                LevelCtrlr.cam.transform.position = new Vector3(0, 0, -(force + cameraAway));
                cameraNotSetOnStart = false;
            }
            if (Input.GetMouseButtonDown(0))
                press = true;
            if (Input.GetKey(KeyCode.Z))
                press = false;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -LevelCtrlr.cam.transform.position.z - force));
            float pzSq = (force * force) - (worldPosition.x * worldPosition.x) - (worldPosition.y * worldPosition.y);
            if (press && pzSq >= 0)
            {
                float pz = -Mathf.Sqrt(pzSq);
                transform.position = new Vector3(worldPosition.x, worldPosition.y, pz);
                transform.LookAt(Vector3.zero);
                LevelCtrlr.cam.transform.position = new Vector3(worldPosition.x, worldPosition.y, -(force + cameraAway));
                LevelCtrlr.cam.transform.LookAt(Vector3.zero);
                lineRenderer.SetPosition(1, transform.position);
                updateRoute();
            }
            else if (press && force <= 0.01f)
            {
                force = 1.0f;
            }
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                force = Mathf.Max(0.1f, force - 0.1f);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                force += 0.1f;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                fired = true;
                rigidbodyDriver.useGravity = true;
                Vector3 distance = -transform.position;
                float magnitude = distance.magnitude;
                Vector3 direction = distance.normalized;
                Vector3 Force = direction * magnitude * rigidbodyDriver.mass;
                GetComponent<RigidbodyDriver>().addForce(RigidbodyDriver.gravity * GetComponent<RigidbodyDriver>().mass, ForceMode.Force);
                GetComponent<RigidbodyDriver>().addForce(Force * 500, ForceMode.Impulse);
                lineRenderer.enabled = false;
            }
        }
        else if (fired && LevelCtrlr.playerView)
        {
            LevelCtrlr.playerView = false;
            LevelCtrlr.once = true;
        }
    }
    public bool hasFired()
    {
        return fired;
    }
    private void updateRoute()
    {
        Vector3 distance = -transform.position;
        float magnitude = distance.magnitude;
        Vector3 direction = distance.normalized;
        Vector3 Force = direction * magnitude * rigidbodyDriver.mass;
        Force *= 500;

        direction.y = 0;
        float4x4 T = float4x4.LookAt(transform.position,
        transform.position + direction, Vector3.up);

        Vector3 acceleration = Force / rigidbodyDriver.mass;
        Vector4 velocity = acceleration * Time.fixedDeltaTime;

        Vector3 velocityT = math.mul(math.inverse(T), velocity).xyz;


        float alpha = Mathf.Deg2Rad * Vector3.Angle(Vector3.forward, velocityT);
        float cosA2 = Mathf.Pow(Mathf.Cos(alpha), 2);
        float tanA = Mathf.Tan(alpha);
        float veloSqrMag = velocity.sqrMagnitude;

        for (int i = 0; i < maxSamples; i++)
        {
            samplesY[i] = -(9.8f * samplesX[i] * samplesX[i] / (2 * veloSqrMag * cosA2)) + samplesX[i] * tanA;
            Vector4 localVec = new Vector3(0, samplesY[i], samplesX[i]);
            localVec.w = 1;
            Vector3 globalVec = math.mul(T, localVec).xyz;

            parabolaRenderer.SetPosition(i, globalVec);
        }

    }
}
