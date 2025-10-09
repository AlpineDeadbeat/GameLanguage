using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class AttachCameraOnSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam) vcam.Follow = transform;
    }
}
