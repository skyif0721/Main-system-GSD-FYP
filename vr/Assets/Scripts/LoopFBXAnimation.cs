using UnityEngine;

/// <summary>
/// Ensures FBX animation plays and loops at runtime.
/// Attach to any FBX model that should continuously animate.
/// Handles both Animator and legacy Animation components.
/// </summary>
public class LoopFBXAnimation : MonoBehaviour
{
    [Tooltip("If true, the animation will loop. If false, plays once.")]
    public bool loop = true;

    private Animator _animator;
    private Animation _legacyAnim;
    private bool _started = false;

    void Start()
    {
        // Try Animator first
        _animator = GetComponent<Animator>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();

        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            _animator.enabled = true;
            _animator.Play(0, 0, 0f);
            _started = true;
            Debug.Log($"[LoopFBX] Playing Animator on {gameObject.name}");
            return;
        }

        // Try legacy Animation
        _legacyAnim = GetComponent<Animation>();
        if (_legacyAnim == null) _legacyAnim = GetComponentInChildren<Animation>();

        if (_legacyAnim != null && _legacyAnim.clip != null)
        {
            _legacyAnim.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            _legacyAnim.Play();
            _started = true;
            Debug.Log($"[LoopFBX] Playing legacy Animation on {gameObject.name}");
            return;
        }

        // If no controller and no legacy animation, log a warning
        if (_animator != null && _animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[LoopFBX] {gameObject.name} has Animator but no RuntimeAnimatorController assigned.");
        }
    }

    void Update()
    {
        if (!_started) return;

        // For Animator, ensure it keeps playing
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (loop && stateInfo.normalizedTime >= 1f)
            {
                _animator.Play(0, 0, 0f);
            }
        }

        // Legacy animation auto-loops via WrapMode
    }
}
