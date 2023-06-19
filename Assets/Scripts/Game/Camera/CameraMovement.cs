using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    private Transform cameraTransform;

    public Transform CameraTarget;

    public float FollowDistance = 30.0f;
    public float MinFollowDistance = 2.0f;

    public float ElevationAngle = 30.0f;
    public float MaxElevationAngle = 85.0f;
    public float MinElevationAngle = 0f;

    public float OrbitalAngle = 0f;

    private Vector3 desiredPosition;
    [SerializeField]
    private float mouseSensitivity = 2.0f;

    void Awake()
    {
        if (QualitySettings.vSyncCount > 0)
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = -1;

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
            Input.simulateMouseWithTouches = false;

        cameraTransform = transform;
    }

    public void Move()
    {
        GetPlayerInput();


        desiredPosition = CameraTarget.position + CameraTarget.TransformDirection(Quaternion.Euler(ElevationAngle, OrbitalAngle, 0f) * (new Vector3(0, 0, -FollowDistance)));

        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, ref LevelCtrlr.currentVelocity, LevelCtrlr.MovementSmoothingValue * Time.fixedDeltaTime);

        cameraTransform.LookAt(CameraTarget);

    }



    void GetPlayerInput()
    {
        float mouseX = -Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        float mouseScroll = Input.mouseScrollDelta.y;
        ElevationAngle = Mathf.Min(ElevationAngle + mouseY, MaxElevationAngle);
        OrbitalAngle += mouseX;
        OrbitalAngle %= 360.0f;
        FollowDistance = Mathf.Max(FollowDistance - mouseScroll, MinFollowDistance);
        if (Input.GetKey(KeyCode.W))
        {
            ElevationAngle = Mathf.Min(ElevationAngle + 1.0f, MaxElevationAngle);
        }
        if (Input.GetKey(KeyCode.S))
        {
            ElevationAngle = Mathf.Max(ElevationAngle - 1.0f, MinElevationAngle);
        }
        if (Input.GetKey(KeyCode.A))
        {
            OrbitalAngle += 1.0f;
            OrbitalAngle %= 360.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            OrbitalAngle -= 1.0f;
            OrbitalAngle %= 360.0f;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            FollowDistance = Mathf.Max(FollowDistance - 0.1f, MinFollowDistance);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            FollowDistance += 0.1f;
        }
    }
}