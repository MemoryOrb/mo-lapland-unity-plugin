using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryOrbLapland;

public class GameManager : MonoBehaviour
{
    private NetworkUtils _network;
    private MemoryOrb _memoryOrb;

    void Start()
    {
        _network = new NetworkUtils();
        _network.OnMessageReceived += NetworkUtils_OnMessageReceived;
        _network.StartServer("55666");
        _network.Listen();
        Debug.Log("StartServer");

        _memoryOrb = new MemoryOrb();
        _memoryOrb.OnButtonChangeState += MemoryOrb_OnButtonChangeState;
        _memoryOrb.OnRotaryButtonChangeState += MemoryOrb_OnRotaryButtonChangeState;
        _memoryOrb.OnRotaryEncoderChangeState += MemoryOrb_OnRotaryEncoderChangeState;
        _memoryOrb.OnPotentiometerChangeState += MemoryOrb_OnPotentiometerChangeState;
    }

    void OnDestroy()
    {
        _network.StopServer();
    }

    private void NetworkUtils_OnMessageReceived(string message)
    {
        // Debug.Log(message);
        _memoryOrb.Feed(message);
    }

    private void MemoryOrb_OnButtonChangeState(Hand h, Finger f, ButtonState b)
    {
        Debug.Log("Button " + h + " " + f + " " + b);
    }

    private void MemoryOrb_OnRotaryButtonChangeState(Hand h, ButtonState b)
    {
        Debug.Log("RotaryButton " + h + " " + b);
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        Debug.Log("Rotary " + h + " " + d);
    }

    private void MemoryOrb_OnPotentiometerChangeState(Potentiometer p, int value)
    {
        Debug.Log("Potentiometer " + p + " " + value);
    }
}
