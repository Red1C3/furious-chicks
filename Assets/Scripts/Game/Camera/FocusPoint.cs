using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusPoint : MonoBehaviour
{
    [SerializeField]
    private float _yawLimit = 45f;

    [SerializeField]
    private float _pitchLimit = 45;

    public float YawLimit { get { return _yawLimit; } }
    public float PitchLimit { get { return _pitchLimit; } }
}