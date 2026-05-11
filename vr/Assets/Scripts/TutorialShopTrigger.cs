using UnityEngine;

/// <summary>
/// Notifies the TutorialManager when the player enters the shop zone.
/// Attach this to the same GameObject as VRShopZone.
/// </summary>
public class TutorialShopTrigger : MonoBehaviour
{
    public TutorialManager tutorialManager;

    private void OnTriggerEnter(Collider other)
    {
        if (tutorialManager == null) return;

        if (other.CompareTag("Player") ||
            other.name.Contains("XR Origin") ||
            other.GetComponentInParent<PlayerStats>() != null)
        {
            tutorialManager.NotifyShopEntered();
        }
    }
}
