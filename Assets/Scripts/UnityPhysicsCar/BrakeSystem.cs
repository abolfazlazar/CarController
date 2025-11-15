using System.Linq;

public class BrakeSystem : ICarComponent
{
    private readonly CarConfig _config;
    private readonly IWheel[] _brakeableWheels;
    private InputData _currentInput;
    
    public BrakeSystem(IWheel[] allWheels, CarConfig config)
    {
        _brakeableWheels = allWheels.Where(w => w.CanBrake).ToArray();
        _config = config;
    }

    public void Update()
    { }

    public void FixedUpdate(float deltaTime)
    {
        ApplyBrakes();
    }
    
    public void SetInput(InputData input)
    {
        _currentInput = input;
    }
    
    private void ApplyBrakes()
    {
        if (_currentInput == null)
            return;
        
        float brakeTorque = _currentInput.Brake * _config.maxBrakeTorque;
        float handbrake = _currentInput.HandBrake ? _config.handbrakeForce : 0f;
        
        float frontBrakeTorque = brakeTorque * _config.brakeBias;
        float rearBrakeTorque = brakeTorque * (1f - _config.brakeBias);
        
        foreach (var wheel in _brakeableWheels)
        {
            wheel.Output.BrakeTorque = 0f; 
            
            if (wheel.CanSteer) 
            {
                wheel.Output.BrakeTorque = frontBrakeTorque;
            }
            else
            {
                wheel.Output.BrakeTorque = rearBrakeTorque + handbrake; 
            }
        }
    }
}