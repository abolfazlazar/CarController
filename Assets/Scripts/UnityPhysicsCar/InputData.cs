// CHANGED: Added interface implementation
public class InputData : ICarInput
{
    // CHANGED: Converted fields to auto-properties with setters 
    public float Throttle { get; set; }
    public float Brake { get; set; }
    public float Steering { get; set; }
    public bool HandBrake { get; set; }
    public float Clutch { get; set; }
}