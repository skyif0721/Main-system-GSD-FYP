using UnityEngine;
using UnityEngine.UI;

public class VRShopZone : MonoBehaviour
{
    public GameObject openShopButtonCanvas;
    public GameObject shopMenuCanvas;
    public GameObject npcDialogueCanvas; // New reference for NPC dialogue
    public Text npcDialogueText;         // New reference for NPC text

    private void Start()
    {
        if (openShopButtonCanvas != null) openShopButtonCanvas.SetActive(false);
        if (shopMenuCanvas != null) shopMenuCanvas.SetActive(false);
        if (npcDialogueCanvas != null) npcDialogueCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is the player
        if (other.CompareTag("Player") || other.name.Contains("XR Origin") || other.GetComponentInParent<PlayerStats>() != null)
        {
            if (openShopButtonCanvas != null) openShopButtonCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object exiting is the player
        if (other.CompareTag("Player") || other.name.Contains("XR Origin") || other.GetComponentInParent<PlayerStats>() != null)
        {
            if (openShopButtonCanvas != null) openShopButtonCanvas.SetActive(false);
            if (shopMenuCanvas != null) shopMenuCanvas.SetActive(false);
            if (npcDialogueCanvas != null) npcDialogueCanvas.SetActive(false);
        }
    }

    public void OpenShop()
    {
        if (openShopButtonCanvas != null) openShopButtonCanvas.SetActive(false);
        if (shopMenuCanvas != null) shopMenuCanvas.SetActive(true);
        
        // Show NPC Dialogue
        if (npcDialogueCanvas != null)
        {
            npcDialogueCanvas.SetActive(true);
            if (npcDialogueText != null)
            {
                npcDialogueText.text = "Welcome to my shop! What can I get for you today?";
            }
        }

        // Switch to shop BGM (mute main BGM)
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.EnableShopBGM();

        // Play button click SFX
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlayButtonClick();
    }

    public void CloseShop()
    {
        if (shopMenuCanvas != null) shopMenuCanvas.SetActive(false);
        if (openShopButtonCanvas != null) openShopButtonCanvas.SetActive(true);
        
        // Hide NPC Dialogue
        if (npcDialogueCanvas != null) npcDialogueCanvas.SetActive(false);

        // Restore main BGM (disable shop BGM)
        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.DisableShopBGM();
    }
}
