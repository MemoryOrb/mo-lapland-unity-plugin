using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryOrbLapland;

public class GameManager : MonoBehaviour
{
    private NetworkUtils network;

    void Start()
    {
        network = new NetworkUtils();
        network.OnMessageReceived += NetworkUtils_OnMessageReceived;
        network.StartServer("55666");
        network.Listen();
        Debug.Log("StartServer");
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        network.StopServer();
    }

    private void NetworkUtils_OnMessageReceived(string message)
    {
        Debug.Log(message);
    }
}
