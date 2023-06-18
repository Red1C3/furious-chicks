using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    // This is the target we'll orbit around
    [SerializeField]
    private FocusPoint _target;

    // Our desired distance from the target object.
    [SerializeField]
    private float _distance = 5;

    [SerializeField]
    private float _damping = 2;

    // These will store our currently desired angles
    private Quaternion _pitch;
    private Quaternion _yaw;

    // this is where we want to go.
    private Quaternion _targetRotation;
    private Vector3 _targetPosition;

    public FocusPoint Target
    {
        get { return _target; }
        set { _target = value; }
    }

    public float Yaw
    {
        get { return _yaw.eulerAngles.y; }
        private set { _yaw = Quaternion.Euler(0, value, 0); }
    }

    public float Pitch
    {
        get { return _pitch.eulerAngles.x; }
        private set { _pitch = Quaternion.Euler(value, 0, 0); }
    }

    public void Move(float yawDelta, float pitchDelta)
    {
        _yaw = _yaw * Quaternion.Euler(0, yawDelta, 0);
        _pitch = _pitch * Quaternion.Euler(pitchDelta, 0, 0);
        ApplyConstraints();
    }

    private void ApplyConstraints()
    {
        Quaternion targetYaw = Quaternion.Euler(0, _target.transform.rotation.eulerAngles.y, 0);
        Quaternion targetPitch = Quaternion.Euler(_target.transform.rotation.eulerAngles.x, 0, 0);

        float yawDifference = Quaternion.Angle(_yaw, targetYaw);
        float pitchDifference = Quaternion.Angle(_pitch, targetPitch);

        float yawOverflow = yawDifference - _target.YawLimit;
        float pitchOverflow = pitchDifference - _target.PitchLimit;

        // We'll simply use lerp to move a bit towards the focus target's orientation. Just enough to get back within the constraints.
        // This way we don't need to worry about wether we need to move left or right, up or down.
        if (yawOverflow > 0) { _yaw = Quaternion.Slerp(_yaw, targetYaw, yawOverflow / yawDifference); }
        if (pitchOverflow > 0) { _pitch = Quaternion.Slerp(_pitch, targetPitch, pitchOverflow / pitchDifference); }
    }

    void Awake()
    {
        // initialise our pitch and yaw settings to our current orientation.
        _pitch = Quaternion.Euler(this.transform.rotation.eulerAngles.x, 0, 0);
        _yaw = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
    }

    public void orbitCameraUpdate()
    {
        // calculate target positions
        _targetRotation = _yaw * _pitch;
        _targetPosition = _target.transform.position + _targetRotation * (-Vector3.forward * _distance);

        // apply movement damping
        // (Yeah I know this is not a mathematically correct use of Lerp. We'll never reach destination. Sue me!)
        // (It doesn't matter because we are damping. We Do Not Need to arrive at our exact destination, we just want to move smoothly and get really, really close to it.)
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _targetRotation, Mathf.Clamp01(Time.smoothDeltaTime * _damping));

        // offset the camera at distance from the target position.
        Vector3 offset = this.transform.rotation * (-Vector3.forward * _distance);
        this.transform.position = _target.transform.position + offset;

        // alternatively, if we desire a slightly different behaviour, we could also add damping to the target position. But this can lead to awkward behaviour if the user rotates quickly or the damping is low.
        //this.transform.position = Vector3.Lerp(this.transform.position, _targetPosition, Mathf.Clamp01(Time.smoothDeltaTime * _damping));
    }
    public void setDistance(float distance){
        _distance=distance;
    }
}