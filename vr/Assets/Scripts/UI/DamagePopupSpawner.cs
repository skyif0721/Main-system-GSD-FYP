using UnityEngine;
using TMPro;

/// <summary>
/// Spawns a floating "-XX HP" text above any monster that takes damage.
/// Call <see cref="Spawn"/> from anywhere (e.g. MonsterStat.TakeDamage)
/// to make the popup appear.
///
/// Also includes a default font / look – no prefab needed; the popup is
/// generated procedurally each call.
/// </summary>
public static class DamagePopupSpawner
{
    /// <summary>Spawn a floating damage number above <paramref name="target"/>.</summary>
    /// <param name="target">World object the popup appears above.</param>
    /// <param name="damage">Damage amount; sign decides color.</param>
    /// <param name="customColor">Optional override color.</param>
    public static void Spawn(Transform target, int damage, Color? customColor = null)
    {
        if (target == null) return;

        // Position 1.6 m above the target's pivot, slightly forward
        Vector3 pos = target.position + Vector3.up * 1.6f;
        // Add a small random horizontal offset so multiple popups don't stack
        pos += new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0f, 0.2f), 0f);

        SpawnAt(pos, damage, customColor);
    }

    /// <summary>Spawn at an exact world position.</summary>
    public static void SpawnAt(Vector3 worldPos, int damage, Color? customColor = null)
    {
        GameObject go = new GameObject($"DamagePopup_{damage}");
        go.transform.position = worldPos;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 4f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineWidth = 0.3f;
        tmp.outlineColor = Color.black;

        // Color: red for damage, green for heal, custom override wins.
        Color c;
        if (customColor.HasValue) c = customColor.Value;
        else if (damage > 0) c = new Color(1f, 0.25f, 0.2f);   // red-ish for damage
        else                  c = new Color(0.4f, 1f, 0.4f);   // green for heal

        tmp.color = c;
        tmp.text = damage > 0 ? $"-{damage} HP" : $"+{-damage} HP";

        // RectTransform size so the text isn't clipped
        RectTransform rt = tmp.rectTransform;
        rt.sizeDelta = new Vector2(4f, 1f);

        FloatingDamageText fdt = go.AddComponent<FloatingDamageText>();
        fdt.textMesh = tmp;
        fdt.Setup(damage);
    }
}
