using UnityEngine;

public interface IWheel
{
    WheelCollider Collider { get; }

    bool IsPowered { get; }
    bool CanSteer { get; }
    bool CanBrake { get; }

    WheelState State { get; set; } 
    WheelOutput Output { get; }

    void ApplyOutputs();
    void UpdateVisuals();
}