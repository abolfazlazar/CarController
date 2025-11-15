using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class CarController : MonoBehaviour
{
    private IWheel[] wheels;
    private Rigidbody rb;
    
    private CarConfig config;
    private InputManager inputManager;
    private EngineSystem engineSystem;
    private SteeringSystem steeringSystem;
    private BrakeSystem brakeSystem;

    public float Speed => rb.linearVelocity.magnitude * 3.6f;

    [Inject]
    private void Construct(
        EngineSystem engineSystem,
        SteeringSystem steeringSystem,
        BrakeSystem brakeSystem,
        CarConfig config,
        InputManager inputManager, IWheel[] wheels)
    {
        this.engineSystem = engineSystem;
        this.steeringSystem = steeringSystem;
        this.brakeSystem = brakeSystem;
        this.config = config;
        this.inputManager = inputManager;
        this.wheels = wheels;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody missing on CarController!");
            enabled = false;
            return;
        }

        // REFACTORED: Use the configurable value from CarConfig instead of a hardcoded magic number (-4)
        rb.centerOfMass = new Vector3(0, config.centerOfMassOffset, 0); 
    }

    private void Update()
    {
        inputManager.UpdateInput();

        // ADDED: Call UpdateVisuals in the Update loop for smooth mesh animation (visuals)
        foreach (var wheel in wheels)
            wheel.UpdateVisuals();

        // REMOVED: Redundant and incorrect call to wheel.ApplyOutputs() which should only run in FixedUpdate 
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        GatherWheelStates();

        InputData input = inputManager.Data;
        ChassisState chassisState = GetChassisState();

        engineSystem.SetInput(input);
        steeringSystem.SetInput(input, chassisState); 
        brakeSystem.SetInput(input);

        engineSystem.FixedUpdate(deltaTime);
        steeringSystem.FixedUpdate(deltaTime);
        brakeSystem.FixedUpdate(deltaTime);

        ApplyWheelOutputs();

        ApplyChassisForces();
        ApplyAntiRollBar();
    }

    private void GatherWheelStates()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.Collider == null) continue;

            float normalizedTravel = 1.0f;
            if (wheel.Collider.GetGroundHit(out WheelHit hit))
            {
                normalizedTravel = (-wheel.Collider.transform.InverseTransformPoint(hit.point).y - wheel.Collider.radius)
                                   / wheel.Collider.suspensionDistance;
            }

            wheel.State = new WheelState
            {
                CurrentRpm = wheel.Collider.rpm,
                SuspensionTravel = normalizedTravel,
            };
        }
    }

    private ChassisState GetChassisState()
    {
        return new ChassisState
        {
            SpeedKmh = Speed,
            ForwardVelocityDot = Vector3.Dot(rb.linearVelocity.normalized, transform.forward),
        };
    }

    private void ApplyWheelOutputs()
    {
        // This is the correct place to call ApplyOutputs (in the physics loop)
        foreach (var wheel in wheels)
        {
            wheel.ApplyOutputs();
        }
    }

    private void ApplyChassisForces()
    {
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        Vector3 dragForce = -velocity.normalized * (config.dragCoefficient * speed * speed);
        rb.AddForce(dragForce, ForceMode.Acceleration);

        float downforce = config.downforceCoefficient * speed * speed;
        rb.AddForce(-transform.up * downforce, ForceMode.Acceleration);
    }

    private void ApplyAntiRollBar()
    {
        if (wheels.Length < 4) return;

        ApplyAntiRollForce(wheels[0].Collider, wheels[1].Collider, config.antiRollForce);
        ApplyAntiRollForce(wheels[2].Collider, wheels[3].Collider, config.antiRollForce);
    }

    private void ApplyAntiRollForce(WheelCollider left, WheelCollider right, float force)
    {
        if (!left || !right) return;

        float travelL = 1f, travelR = 1f;

        bool groundedL = left.GetGroundHit(out WheelHit hitL);
        if (groundedL)
            travelL = (-left.transform.InverseTransformPoint(hitL.point).y - left.radius) / left.suspensionDistance;

        bool groundedR = right.GetGroundHit(out WheelHit hitR);
        if (groundedR)
            travelR = (-right.transform.InverseTransformPoint(hitR.point).y - right.radius) / right.suspensionDistance;

        float rollForce = (travelL - travelR) * force;

        if (groundedL)
            rb.AddForceAtPosition(left.transform.up * -rollForce, left.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(right.transform.up * rollForce, right.transform.position);
    }

    private void OnGUI()
    {
        if (engineSystem == null) return;

        inputManager.DrawDebugGUI();

        GUI.Box(new Rect(10, 130, 280, 160), "Car Stats");
        GUI.Label(new Rect(20, 150, 280, 20), $"Speed: {Speed:F1} km/h");
        GUI.Label(new Rect(20, 170, 280, 20), $"RPM: {engineSystem.CurrentRpm:F0}");
        GUI.Label(new Rect(20, 190, 280, 20), $"Gear: {engineSystem.CurrentGear}");
        GUI.Label(new Rect(20, 210, 280, 20), $"Torque: {engineSystem.OutputTorque:F0} Nm");

        int powered = wheels.Count(w => w.IsPowered);
        GUI.Label(new Rect(20, 230, 280, 20), $"Powered Wheels: {powered}");

        if (wheels.Length > 0 && wheels[0].Collider != null)
        {
            GUI.Label(new Rect(20, 250, 280, 20), $"Wheel RPM: {wheels[0].Collider.rpm:F0}");
            int grounded = wheels.Count(w => w.Collider.isGrounded);
            GUI.Label(new Rect(20, 270, 280, 20), $"Grounded: {grounded}/{wheels.Length}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !rb) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rb.transform.TransformPoint(rb.centerOfMass), 0.1f);

        foreach (var wheel in wheels)
        {
            if (wheel.Collider == null) continue;

            if (wheel.Collider.GetGroundHit(out WheelHit hit))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(wheel.Collider.transform.position, 0.2f);
            }
        }
    }
}