using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetStateDebug : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;
        var map = GetComponent<PlayerInput>()?.currentActionMap?.name ?? "<none>";
        var k = Keyboard.current;
        var v = new Vector2(
          (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
          (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)
        );
        if (v != Vector2.zero) Debug.Log($"[Owner] Map:{map} Raw:{v}");
    }
}