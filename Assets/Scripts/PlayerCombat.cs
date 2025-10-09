using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee")]
    public int damage = 3;
    public float range = 1.2f;
    public float arcDegrees = 90f;
    public LayerMask enemyLayers;

    [Header("FX (optional)")]
    public AudioClip swingSfx;

    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || PauseController.IsGamePaused) return;

        if (swingSfx) SoundEffectManager.Play(swingSfx.name, true);
        if (_anim) _anim.SetTrigger("attack");

        // Direction from animator’s last input (same approach as movement)
        Vector2 dir = new Vector2(_anim.GetFloat("LastInputX"), _anim.GetFloat("LastInputY"));
        if (dir == Vector2.zero)
        {
            // fallback to current input if we’re moving
            dir = new Vector2(_anim.GetFloat("InputX"), _anim.GetFloat("InputY"));
            if (dir == Vector2.zero) dir = Vector2.down; // default facing
        }
        dir.Normalize();

        // Overlap and cone-filter
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position, range, enemyLayers);
        float halfArc = arcDegrees * 0.5f;

        foreach (var h in hits)
        {
            Vector2 to = (Vector2)h.transform.position - (Vector2)transform.position;
            float angle = Vector2.Angle(dir, to);
            if (angle <= halfArc)
            {
                var enemy = h.GetComponent<Enemy>();
                if (enemy != null) enemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
