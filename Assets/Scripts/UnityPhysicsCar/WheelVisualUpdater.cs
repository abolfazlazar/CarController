using UnityEngine;

public class WheelVisualUpdater : MonoBehaviour
{
    [SerializeField] private WheelCollider wheelCollider;

    private void LateUpdate()
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        transform.position = pos;
        transform.rotation = rot;;
    }
}
