using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central audio manager that handles all game SFX and BGM.
/// Manages: weapon sounds, button clicks, drink sounds, monster sounds,
/// player movement sounds, and BGM transitions (mute main BGM when shop opens).
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("BGM Clips")]
    public AudioClip bgmMenu;          // peaceful life - tutorials/main menu
    public AudioClip bgmShop;          // IntactSun - shop
    public AudioClip bgmExplore;       // TheBlessedOne - no monster nearby
    public AudioClip bgmCombat;        // TheCursedOne - fighting
    public AudioClip bgmBoss;          // Yaoguai Mountain - boss fight

    [Header("SFX Clips")]
    public AudioClip sfxButtonClick;   // button clicks
    public AudioClip sfxSwordHit;      // Sword Sound Effect
    public AudioClip sfxWeaponDrop;    // metal pipe falling
    public AudioClip sfxDrink;         // Roblox Drinking Sound Effect
    public AudioClip sfxPunch;         // Punch - for all movement
    public AudioClip sfxFireball;      // Fire - add to fire ball
    public AudioClip sfxMonsterSpawn;  // GET OVER HERE
    public AudioClip sfxMonsterShout;  // Monster sound when starting
    public AudioClip sfxBossShout;     // Final Boss - when boss is here

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("BGM Fade")]
    public float bgmFadeDuration = 1.0f;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private AudioSource _shopBgmSource;
    private bool _shopBgmActive = false;
    private AudioClip _currentBgm;
    private Coroutine _fadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = bgmVolume;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = sfxVolume;

        _shopBgmSource = gameObject.AddComponent<AudioSource>();
        _shopBgmSource.loop = true;
        _shopBgmSource.playOnAwake = false;
        _shopBgmSource.volume = 0f;
    }

    // ─── BGM Control ──────────────────────────────────────────────────────

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || clip == _currentBgm) return;
        _currentBgm = clip;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeBGM(clip));
    }

    public void PlayMenuBGM() => PlayBGM(bgmMenu);
    public void PlayExploreBGM() => PlayBGM(bgmExplore);
    public void PlayCombatBGM() => PlayBGM(bgmCombat);
    public void PlayBossBGM() => PlayBGM(bgmBoss);

    IEnumerator CrossfadeBGM(AudioClip newClip)
    {
        float timer = 0f;
        float startVol = _bgmSource.volume;

        // Fade out current
        while (timer < bgmFadeDuration)
        {
            timer += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVol, 0f, timer / bgmFadeDuration);
            yield return null;
        }

        _bgmSource.clip = newClip;
        _bgmSource.Play();

        // Fade in new
        timer = 0f;
        float targetVol = _shopBgmActive ? 0f : bgmVolume;
        while (timer < bgmFadeDuration)
        {
            timer += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, targetVol, timer / bgmFadeDuration);
            yield return null;
        }
        _bgmSource.volume = targetVol;
    }

    /// <summary>
    /// Called when shop opens - mutes main BGM and plays shop BGM
    /// </summary>
    public void EnableShopBGM()
    {
        if (_shopBgmActive) return;
        _shopBgmActive = true;

        if (bgmShop != null)
        {
            _shopBgmSource.clip = bgmShop;
            _shopBgmSource.Play();
        }

        StartCoroutine(FadeAudioSource(_bgmSource, _bgmSource.volume, 0f, bgmFadeDuration));
        StartCoroutine(FadeAudioSource(_shopBgmSource, 0f, bgmVolume, bgmFadeDuration));
    }

    /// <summary>
    /// Called when shop closes - restores main BGM
    /// </summary>
    public void DisableShopBGM()
    {
        if (!_shopBgmActive) return;
        _shopBgmActive = false;

        StartCoroutine(FadeAudioSource(_shopBgmSource, _shopBgmSource.volume, 0f, bgmFadeDuration));
        StartCoroutine(FadeAudioSource(_bgmSource, 0f, bgmVolume, bgmFadeDuration));
    }

    IEnumerator FadeAudioSource(AudioSource source, float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        source.volume = to;
        if (to <= 0.01f) source.Stop();
    }

    // ─── SFX ──────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick() => PlaySFX(sfxButtonClick);
    public void PlaySwordHit() => PlaySFX(sfxSwordHit);
    public void PlayWeaponDrop() => PlaySFX(sfxWeaponDrop);
    public void PlayDrink() => PlaySFX(sfxDrink);
    public void PlayPunch() => PlaySFX(sfxPunch);
    public void PlayFireball() => PlaySFX(sfxFireball);
    public void PlayMonsterSpawn() => PlaySFX(sfxMonsterSpawn);
    public void PlayMonsterShout() => PlaySFX(sfxMonsterShout);
    public void PlayBossShout() => PlaySFX(sfxBossShout);

    /// <summary>Play a one-shot SFX at a world position (3D spatialized)</summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }

    public void StopAllBGM()
    {
        _bgmSource.Stop();
        _shopBgmSource.Stop();
        _shopBgmActive = false;
    }
}
