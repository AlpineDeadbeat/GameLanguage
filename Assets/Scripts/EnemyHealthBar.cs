using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fill;

    public void SetHealth(int current, int max)
    {
        if (fill == null || max <= 0) return;
        fill.fillAmount = Mathf.Clamp01((float)current / max);
    }
}