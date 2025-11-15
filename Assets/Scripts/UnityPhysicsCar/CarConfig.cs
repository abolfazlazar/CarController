using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CarConfig", menuName = "CarConfig")]
public class CarConfig : ScriptableObject
{
    [Header("Engine")] public AnimationCurve engineTorqueCurve = new AnimationCurve(
        new Keyframe(800, 500, 0, 0), // Idle: 500 Nm
        new Keyframe(4500, 800, 0, 0), // Peak Torque: 800 Nm @ 4500 RPM
        new Keyframe(7000, 650, 0, 0) // Max RPM: 650 Nm
    );
    public float maxRpm = 7000f;
    public float idleRpm = 800f;
    public float engineInertia = 0.3f; 
    [Range(1f, 10f)]
    public float powerMultiplier = 8f; // <-- INCREASED (was 3f) to overcome low speed cap and drag
    
    [Header("Transmission")]
    public float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f }; 
    public float reverseGearRatio = 3.0f;
    public float finalDriveRatio = 3.0f; // <-- DECREASED (was 3.42f) for longer overall gearing and higher top speed
    public float shiftUpRpm = 6000f;
    public float shiftDownRpm = 2500f;
    public float shiftClutchTime = 0.15f; 
    public float clutchValueDuringShift = 0.3f; 
    public const float ShiftDelay = 0.5f; 
    
    [Header("Steering")]
    public float maxSteeringAngle = 35f;
    public AnimationCurve steeringCurve = AnimationCurve.Linear(0, 1, 100, 0.3f); 
    public float steeringSpeed = 3f; 
    
    [Header("Brakes")]
    public float maxBrakeTorque = 3000f;
    public float handbrakeForce = 5000f;
    [Range(0f, 1f)]
    public float brakeBias = 0.6f; 
    
    [Header("Physics (Unity Wrapper Use Only)")]
    public float centerOfMassOffset = -0.5f;

    public float downforceCoefficient = 2f;
    public float dragCoefficient = 0.15f; // <-- DECREASED (was 0.2f) to reduce terminal velocity cap
    public float antiRollForce = 5000f;
}