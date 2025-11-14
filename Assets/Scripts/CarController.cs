using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public float motorPower = 1500f;
    public float steerAngle = 30f;

    void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        wheelFL.steerAngle = h * steerAngle;
        wheelFR.steerAngle = h * steerAngle;

        wheelRL.motorTorque = v * motorPower;
        wheelRR.motorTorque = v * motorPower;
    }
}