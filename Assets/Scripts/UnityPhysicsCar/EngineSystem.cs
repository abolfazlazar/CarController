using System.Linq;
using UnityEngine;
using Zenject;

public class EngineSystem : ICarComponent
{
    private CarConfig _config;
    private readonly IWheel[] _poweredWheels;

    public float CurrentRpm { get; private set; }
    public int CurrentGear { get; private set; }
    public float OutputTorque { get; private set; }
    
    private float _clutch = 1f;
    private float _lastShiftTime = 0f;
    private float _currentShiftTime = 0f;

    private InputData _currentInput;
    
    // Debug tracking
    private float _debugTimer = 0f;
    
    public EngineSystem(IWheel[] allWheels, CarConfig config)
    {
        _poweredWheels = allWheels.Where(w => w.IsPowered).ToArray();
        
        _config = config;
        
        Debug.Log($"EngineSystem: Found {_poweredWheels.Length} powered wheels out of {allWheels.Length} total");
        
        Initialize();
    }

    public void Initialize() 
    {
        CurrentRpm = _config.idleRpm;
        CurrentGear = 1;
        _lastShiftTime = -_config.shiftClutchTime; 
    }

    public void Update()
    { }

    public void FixedUpdate(float deltaTime)
    {
        UpdateClutchState(deltaTime);
        CalculateEngineDynamics(deltaTime);
        
        // Debug output every 0.5 seconds
        _debugTimer += deltaTime;
        if (_debugTimer > 0.5f)
        {
            _debugTimer = 0f;
            var wheelRpm = CalculateAverageWheelRPM();
            Debug.Log($"RPM: {CurrentRpm:F0} | Gear: {CurrentGear} | Torque: {OutputTorque:F0} | WheelRPM: {wheelRpm:F0} | Clutch: {_clutch:F2} | Throttle: {_currentInput?.Throttle:F2}");
        }
    }

    public void SetInput(InputData input)
    {
        _currentInput = input;
    }

    private void UpdateClutchState(float deltaTime)
    {
        if (!(_currentShiftTime > 0)) return;
        
        _currentShiftTime -= deltaTime;
        if (_currentShiftTime <= 0)
        {
            _clutch = 1f;
        }
    }

    private void CalculateEngineDynamics(float deltaTime)
    {
        if (_currentInput == null)
            return;

        var wheelRpm = CalculateAverageWheelRPM();

        var gearRatio = CurrentGear == 0 ? _config.reverseGearRatio : 
                          CurrentGear > 0 && CurrentGear <= _config.gearRatios.Length ? _config.gearRatios[CurrentGear - 1] : 0f;

        if (gearRatio == 0f && CurrentGear != 0)
        {
            Debug.LogWarning($"EngineSystem: Invalid gear {CurrentGear}, resetting to gear 1");
            CurrentGear = 1;
            gearRatio = _config.gearRatios[0];
        }

        // Calculate what RPM the wheels are demanding from the engine
        float wheelDemandRpm = Mathf.Abs(wheelRpm) * gearRatio * _config.finalDriveRatio;
        
        // Calculate desired engine RPM based on throttle input
        float throttleRpm = Mathf.Lerp(_config.idleRpm, _config.maxRpm, _currentInput.Throttle);
        
        // The engine tries to reach throttle RPM, but is also influenced by wheel load
        float targetRPM;
        
        if (Mathf.Abs(wheelRpm) < 5f)
        {
            // At standstill, use pure throttle input
            targetRPM = throttleRpm;
        }
        else
        {
            // Blend between wheel demand and throttle demand
            // This allows engine to rev ahead of wheels
            targetRPM = Mathf.Max(wheelDemandRpm, throttleRpm * 0.85f);
        }
        
        float targetRpmClamped = Mathf.Clamp(targetRPM, _config.idleRpm, _config.maxRpm);
        
        // Engine inertia - how quickly RPM responds
        float rpmDelta = (targetRpmClamped - CurrentRpm) * (deltaTime / Mathf.Max(0.001f, _config.engineInertia));
        
        // Limit RPM change rate for realism
        float maxRpmChange = 5000f * deltaTime;
        rpmDelta = Mathf.Clamp(rpmDelta, -maxRpmChange, maxRpmChange);
        
        CurrentRpm = Mathf.Clamp(CurrentRpm + rpmDelta, _config.idleRpm, _config.maxRpm);

        AutomaticTransmission();

        // Calculate engine torque from curve
        float baseTorque = _config.engineTorqueCurve.Evaluate(CurrentRpm) * _currentInput.Throttle;

        // Final output torque through transmission
        OutputTorque = baseTorque * gearRatio * _config.finalDriveRatio * _clutch * _config.powerMultiplier;

        // Apply torque to wheels
        if (_poweredWheels.Length > 0)
        {
            float torquePerWheel = OutputTorque / _poweredWheels.Length;
            foreach (var wheel in _poweredWheels)
            {
                if (wheel != null && wheel.Output != null)
                {
                    wheel.Output.MotorTorque = CurrentGear == 0 ? -torquePerWheel : torquePerWheel;
                }
            }
        }
        else
        {
            Debug.LogWarning("No powered wheels to apply torque to!");
        }
    }
    
    private float CalculateAverageWheelRPM()
    {
        float totalAbsRpm = 0f;
        int validWheels = 0;
        
        foreach (var wheel in _poweredWheels)
        {
            if (wheel != null)
            {
                totalAbsRpm += Mathf.Abs(wheel.State.CurrentRpm);
                validWheels++;
            }
        }
        
        return validWheels > 0 ? totalAbsRpm / validWheels : 0f;
    }
    
    private void AutomaticTransmission()
    {
        if (Time.time - _lastShiftTime < CarConfig.ShiftDelay) 
            return;
        
        bool shouldShift = false;
        float avgRpm = CurrentRpm;
        float avgWheelRpm = CalculateAverageWheelRPM();

        if (avgRpm > _config.shiftUpRpm && CurrentGear > 0 && CurrentGear < _config.gearRatios.Length)
        {
            CurrentGear++;
            shouldShift = true;
            Debug.Log($"Shifting UP to gear {CurrentGear}");
        }
        else if (avgRpm < _config.shiftDownRpm && CurrentGear > 1)
        {
            CurrentGear--;
            shouldShift = true;
            Debug.Log($"Shifting DOWN to gear {CurrentGear}");
        }
        else if (CurrentGear > 0 && avgWheelRpm < 5f && _currentInput.Brake > 0.5f)
        {
            CurrentGear = 0;
            shouldShift = true;
            Debug.Log("Shifting to REVERSE");
        }
        else if (CurrentGear == 0 && _currentInput.Throttle > 0.1f && avgWheelRpm < 5f)
        {
            CurrentGear = 1;
            shouldShift = true;
            Debug.Log("Shifting to gear 1 from reverse");
        }

        if (shouldShift)
        {
            _lastShiftTime = Time.time;
            _clutch = _config.clutchValueDuringShift;
            _currentShiftTime = _config.shiftClutchTime;
        }
    }
}