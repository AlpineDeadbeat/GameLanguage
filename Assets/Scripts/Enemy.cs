using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Identity")]
    public int enemyID = 1;            // Tie this to QuestObjective.objectiveID
    public string enemyName = "Slime";

    [Header("Stats")]
    public int maxHealth = 10;
    public float hitFlashTime = 0.1f;

    [Header("Loot")]
    public List<LootEntry> lootTable = new(); // Configure in Inspector

    [Header("FX (optional)")]
    public AudioClip hitSfx;
    public AudioClip deathSfx;

    private int _currentHealth;
    private SpriteRenderer _sr;
    private Color _baseColor;

    void Awake()
    {
        _currentHealth = maxHealth;
        _sr = GetComponent<SpriteRenderer>();
        if (_sr) _baseColor = _sr.color;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || _currentHealth <= 0) return;

        _currentHealth -= amount;
        if (hitSfx) SoundEffectManager.Play(hitSfx.name, true);
        Flash();

        EnemyHealthBar hb = GetComponentInChildren<EnemyHealthBar>();
        if (hb) hb.SetHealth(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Flash()
    {
        if (_sr == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashCR());
    }

    System.Collections.IEnumerator FlashCR()
    {
        _sr.color = Color.white;
        yield return new WaitForSeconds(hitFlashTime);
        _sr.color = _baseColor;
    }

    void Die()
    {
        if (deathSfx) SoundEffectManager.Play(deathSfx.name, true);

        // 1) Drop loot
        DropLoot();

        // 2) Quest progress: DefeatEnemy objectives where objectiveID == enemyID
        if (QuestController.Instance != null)
        {
            foreach (var qp in QuestController.Instance.activateQuests)
            {
                foreach (var obj in qp.objectives)
                {
                    if (obj.type != ObjectiveType.DefeatEnemy) continue;
                    if (!int.TryParse(obj.objectiveID, out int id)) continue;
                    if (id != enemyID) continue;

                    obj.currentAmount = Mathf.Min(obj.currentAmount + 1, obj.requiredAmount);
                }
            }
            // Refresh quest UI
            FindObjectOfType<QuestUI>()?.UpdateQuestUI();
        }

        Destroy(gameObject);
    }
    [Header("Behaviour")]
    public bool dieOnPlayerContact = true;

    // If your enemy has a solid collider (Is Trigger = false)
    void OnCollisionEnter2D(Collision2D c)
    {
        if (!dieOnPlayerContact) return;
        if (c.collider.CompareTag("Player"))
        {
            _currentHealth = 0;   // ensure <= 0
            Die();                // drops loot, updates quests, destroys self
        }
    }

    // If you use a trigger collider (e.g., a child “Hurtbox”)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!dieOnPlayerContact) return;
        if (other.CompareTag("Player"))
        {
            _currentHealth = 0;
            Die();
        }
    }

    void DropLoot()
    {
        var dict = FindAnyObjectByType<ItemDictionary>();
        if (dict == null) return;

        foreach (var entry in lootTable)
        {
            if (Random.value > entry.dropChance) continue;

            int amount = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
            amount = Mathf.Max(1, amount);

            GameObject itemPrefab = dict.GetItemPrefab(entry.itemID);
            if (itemPrefab == null) continue;

            for (int i = 0; i < amount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * 0.6f;
                GameObject drop = Instantiate(itemPrefab, transform.position + (Vector3)offset, Quaternion.identity);
                var bounce = drop.GetComponent<BounceEffect>();
                if (bounce) bounce.StartBounce();
            }
        }
    }
}

[System.Serializable]
public class LootEntry
{
    public int itemID;          // Must exist in ItemDictionary
    [Range(0f, 1f)] public float dropChance = 0.5f;
    public int minQuantity = 1;
    public int maxQuantity = 1;
}
