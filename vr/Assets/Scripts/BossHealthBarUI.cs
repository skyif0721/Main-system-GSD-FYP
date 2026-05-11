using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a boss/monster health bar fixed to the top of the player's screen.
/// Attach to the PlayerUICanvas or any screen-space overlay canvas.
/// Automatically finds the first MonsterStat in the scene and tracks its HP.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthText;
    public GameObject healthBarPanel;

    [Header("Settings")]
    public string bossDisplayName = "BOSS";
    public bool autoFindMonster = true;

    private MonsterStat _trackedMonster;
    private int _lastKnownHealth = -1;

    void Start()
    {
        if (autoFindMonster)
            FindMonster();

        if (healthBarPanel != null)
            healthBarPanel.SetActive(_trackedMonster != null);
    }

    void Update()
    {
        if (_trackedMonster == null)
        {
            if (autoFindMonster)
            {
                FindMonster();
                if (_trackedMonster == null)
                {
                    if (healthBarPanel != null && healthBarPanel.activeSelf)
                        healthBarPanel.SetActive(false);
                    return;
                }
            }
            else return;
        }

        if (healthBarPanel != null && !healthBarPanel.activeSelf)
            healthBarPanel.SetActive(true);

        // Only update UI when health changes
        if (_trackedMonster.health != _lastKnownHealth)
        {
            _lastKnownHealth = _trackedMonster.health;
            UpdateUI();
        }
    }

    void FindMonster()
    {
        _trackedMonster = FindObjectOfType<MonsterStat>();
        if (_trackedMonster != null)
        {
            _lastKnownHealth = _trackedMonster.health;
            if (healthSlider != null)
                healthSlider.maxValue = _trackedMonster.health;
            UpdateUI();
        }
    }

    public void SetMonster(MonsterStat monster, string displayName = null)
    {
        _trackedMonster = monster;
        if (displayName != null) bossDisplayName = displayName;
        if (monster != null)
        {
            _lastKnownHealth = monster.health;
            if (healthSlider != null)
                healthSlider.maxValue = monster.health;
            UpdateUI();
        }
        if (healthBarPanel != null)
            healthBarPanel.SetActive(monster != null);
    }

    void UpdateUI()
    {
        if (_trackedMonster == null) return;

        if (healthSlider != null)
            healthSlider.value = Mathf.Max(0, _trackedMonster.health);

        if (healthText != null)
            healthText.text = Mathf.Max(0, _trackedMonster.health) + " / " + (int)healthSlider.maxValue;

        if (bossNameText != null)
            bossNameText.text = bossDisplayName;
    }
}
