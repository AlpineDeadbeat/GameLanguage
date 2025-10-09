using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetBootstrap : MonoBehaviour
{
    [Header("Optional, assign in Inspector")]
    public NetworkManager networkManager;
    public UnityTransport transport;

    void Awake()
    {
        if (networkManager == null) networkManager = NetworkManager.Singleton;
        if (transport == null && networkManager != null) transport = networkManager.GetComponent<UnityTransport>();
    }

    public void StartHost()
    {
        if (networkManager != null && !networkManager.IsClient && !networkManager.IsServer)
            networkManager.StartHost();
    }

    public void StartClient()
    {
        if (networkManager != null && !networkManager.IsClient && !networkManager.IsServer)
            networkManager.StartClient();
    }

    public void StartServer()
    {
        if (networkManager != null && !networkManager.IsClient && !networkManager.IsServer)
            networkManager.StartServer();
    }
}
