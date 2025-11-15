public interface ICarInput
{
    float Throttle { get; }
    float Brake { get; }
    float Steering { get; }
    
    // CHANGED: Renamed from 'Handbrake' to 'HandBrake' for consistency
    bool HandBrake { get; } 
    
    float Clutch { get; }
}