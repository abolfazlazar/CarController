using System.Linq;
using UnityEngine;

public class SteeringSystem : ICarComponent
{
    private readonly CarConfig _config;
    private readonly IWheel[] _steerableWheels;

    private float _currentSteeringAngle;

    private InputData _lastInput = new InputData();
    private ChassisState _chassisState = new ChassisState();

    public SteeringSystem(IWheel[] allWheels, CarConfig config)
    {
        _steerableWheels = allWheels.Where(w => w.CanSteer).ToArray();
        _config          = config;
    }

    public void Update() { }

    public void FixedUpdate(float deltaTime)
    {
        if (_config == null) return;

        CalculateSteering(deltaTime);
    }

    public void SetInput(InputData input, ChassisState state)
    {
        _lastInput = input;
        _chassisState = state;
    }

    private void CalculateSteering(float deltaTime)
    {
        float speedFactor = _config.steeringCurve != null
            ? _config.steeringCurve.Evaluate(_chassisState.SpeedKmh)
            : 1f;

        float maxSteer   = _config.maxSteeringAngle * speedFactor;
        float targetAngle = _lastInput.Steering * maxSteer;

        _currentSteeringAngle = Mathf.Lerp(
            _currentSteeringAngle,
            targetAngle,
            deltaTime * _config.steeringSpeed);

        foreach (var wheel in _steerableWheels)
        {
            if (wheel?.Output != null)
                wheel.Output.SteerAngle = _currentSteeringAngle;
        }
    }
}