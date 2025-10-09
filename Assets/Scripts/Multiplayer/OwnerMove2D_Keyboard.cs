using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class OwnerMove2D_Keyboard : NetworkBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb; Vector2 input;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        if (!IsOwner) return;
        var k = Keyboard.current; if (k == null) return;
        input = Vector2.zero;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) input.y += 1;
        if (k.sKey.isPressed || k.downArrowKey.isPressed) input.y -= 1;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) input.x -= 1;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) input.x += 1;
        input = input.normalized;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        rb.linearVelocity = input * speed;
    }
}