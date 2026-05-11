using UnityEngine;
using System.Collections;

/// <summary>
/// Boss shouts when the player enters the mountain top area (boss arena).
/// Also fills the boss HP bar with an animation when entering.
/// Attach to a trigger collider at the top of the mountain.
/// </summary>
public class BossEntranceShout : MonoBehaviour
{
    [Header("Boss Reference")]
    public MonsterStat bossMonster;
    public BossHealthBarUI bossHealthBar;

    [Header("HP Fill Animation")]
    public float hpFillDuration = 2f;

    [Header("Shout Settings")]
    public AudioClip bossShoutClip;
    public float shoutDelay = 0.5f;

    private bool _triggered = false;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0.5f;
        _audioSource.volume = 1f;

        // Auto-find boss if not assigned
        if (bossMonster == null)
            bossMonster = FindObjectOfType<MonsterStat>();
        if (bossHealthBar == null)
            bossHealthBar = FindObjectOfType<BossHealthBarUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        // Check if player entered
        if (other.CompareTag("Player") || other.name.Contains("XR Origin") ||
            other.GetComponentInParent<PlayerStats>() != null)
        {
            _triggered = true;
            StartCoroutine(BossEntrance());
        }
    }

    IEnumerator BossEntrance()
    {
        yield return new WaitForSeconds(shoutDelay);

        // Play boss shout
        if (bossShoutClip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(bossShoutClip);
        }
        else if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.PlayBossShout();
        }

        // Switch to boss BGM
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayBossBGM();

        // Animate HP bar filling up
        if (bossMonster != null && bossHealthBar != null)
        {
            StartCoroutine(FillHPBar());
        }

        Debug.Log("[BossEntranceShout] Boss has appeared! ROAR!");
    }

    IEnumerator FillHPBar()
    {
        if (bossHealthBar.healthSlider == null) yield break;

        int maxHP = bossMonster.health;
        bossHealthBar.healthSlider.maxValue = maxHP;
        bossHealthBar.healthSlider.value = 0;

        if (bossHealthBar.healthBarPanel != null)
            bossHealthBar.healthBarPanel.SetActive(true);

        float timer = 0f;
        while (timer < hpFillDuration)
        {
            timer += Time.deltaTime;
            float t = timer / hpFillDuration;
            // Ease in-out
            t = t * t * (3f - 2f * t);
            bossHealthBar.healthSlider.value = Mathf.Lerp(0, maxHP, t);

            if (bossHealthBar.healthText != null)
                bossHealthBar.healthText.text = Mathf.RoundToInt(bossHealthBar.healthSlider.value) + " / " + maxHP;

            yield return null;
        }

        bossHealthBar.healthSlider.value = maxHP;
        Debug.Log("[BossEntranceShout] HP bar fill complete!");
    }
}
