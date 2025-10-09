using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveNetSpeed = 5f;
    Rigidbody2D ridgedb; Vector2 serverInput;
    void Awake() { ridgedb = GetComponent<Rigidbody2D>(); }

    // Hook via PlayerInput or call manually from your UI
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;
        if (ctx.performed || ctx.canceled) SubmitInputServerRpc(ctx.ReadValue<Vector2>());
    }

    [ServerRpc] void SubmitInputServerRpc(Vector2 input) { serverInput = input; }

    [System.Obsolete]
    void FixedUpdate() { if (IsServer) ridgedb.velocity = serverInput * moveNetSpeed; }
}
