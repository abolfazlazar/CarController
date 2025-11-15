using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : MonoBehaviour
{
    private InputSystem_Actions _actions;
    private InputSystem_Actions.PlayerActions _player;

    public InputData Data { get; private set; }

    private void Awake()
    {
        _actions = new InputSystem_Actions();
        _player  = _actions.Player;

        Data = new InputData();
    }

    private void OnEnable()
    {
        _player.Enable();
    }

    private void OnDisable()
    {
        _player.Disable();
    }

    private void OnDestroy()
    {
        _actions?.Dispose();
    }

    public void UpdateInput()
    {
        Data.Throttle   = _player.Throttle.ReadValue<float>();
        Data.Brake      = _player.Brake.ReadValue<float>();
        Data.Steering   = _player.Steer.ReadValue<float>();
        Data.HandBrake  = _player.HandBrake.ReadValue<float>() != 0; 
        Data.Clutch     = 0f;
    }

    public void DrawDebugGUI()
    {
        GUI.Box(new Rect(10, 10, 280, 120), "Input Debug");
        GUI.Label(new Rect(20, 30, 250, 20), $"Throttle:   {Data.Throttle:F2} (W)");
        GUI.Label(new Rect(20, 50, 250, 20), $"Brake:      {Data.Brake:F2} (S)");
        GUI.Label(new Rect(20, 70, 250, 20), $"Steering:   {Data.Steering:F2} (A/D)");
        GUI.Label(new Rect(20, 90, 250, 20), $"HandBrake:  {Data.HandBrake:F0} (Space)");
    }

    private void OnGUI()
    {
        DrawDebugGUI();
    }
}