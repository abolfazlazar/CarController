using System;
using UnityEngine;

[Serializable]
public class Wheel : IWheel
{
    [SerializeField] private WheelCollider wheelCollider;
    [SerializeField] private Transform wheelMesh;

    [SerializeField] private bool isPowered;
    [SerializeField] private bool canSteer;
    [SerializeField] private bool canBrake;

    public bool IsPowered => isPowered;
    public bool CanSteer => canSteer;
    public bool CanBrake => canBrake;

    public WheelState State { get; set; }
    public WheelOutput Output { get; private set; } = new();

    public WheelCollider Collider => wheelCollider;

    public void UpdateVisuals()
    {
        if (wheelMesh == null || wheelCollider == null) return;
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        wheelMesh.position = pos;
        wheelMesh.rotation = rot;
    }

    public void ApplyOutputs()
    {
        if (wheelCollider == null) return;

        wheelCollider.steerAngle = Output.SteerAngle;

        wheelCollider.motorTorque = IsPowered ? Output.MotorTorque : 0f;

        wheelCollider.brakeTorque = CanBrake ? Output.BrakeTorque : 0f;
    }
}