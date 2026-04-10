using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager.Requests;
using Ink.Runtime;

public class DialoguePanelUi : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private DialogueChoiceButton[] choiceButtons;

    private void Awake()
    {
        contentParent.SetActive(false);
        ResetPanel();
    }

    private void OnEnable()
    {
        var gem = GameEventsManager.instance;
        if (gem?.dialogueEvents != null)
        {
            gem.dialogueEvents.onDialogueStarted += DialogueStarted;
            gem.dialogueEvents.onDialogueFinished += DialogueFinished;
            gem.dialogueEvents.onDisplayDialogue += DisplayDialogue;
        }
        else
        {
            Debug.LogWarning($"{name}: GameEventsManager.instance or inputEvents is null in OnEnable.");
        }
    }

    private void OnDisable()
    {
        var gem = GameEventsManager.instance;
        if (gem?.dialogueEvents != null)
        {
            gem.dialogueEvents.onDialogueStarted += DialogueStarted;
            gem.dialogueEvents.onDialogueFinished += DialogueFinished;
            gem.dialogueEvents.onDisplayDialogue += DisplayDialogue;
        }
    }

    private void DialogueStarted()
    {
        contentParent.SetActive(true);
    }

    private void DialogueFinished()
    {
        contentParent.SetActive(false);

        ResetPanel();
    }

    private void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices)
    {
        dialogueText.text = dialogueLine;

        if(dialogueChoices.Count > choiceButtons.Length)
        {
            Debug.LogError("More Dialougue choice (" + dialogueChoices.Count + 
                ") came through than are supported (" + choiceButtons.Length + ").");
        }

        foreach (DialogueChoiceButton choiceButton in choiceButtons)
        {
            choiceButton.gameObject.SetActive(false);
        }

        int choniceBuutonIndex = dialogueChoices.Count - 1;
        for(int inkChoiceIndex = 0; inkChoiceIndex < dialogueChoices.Count; inkChoiceIndex++)
        {
            Choice dialogueChoice = dialogueChoices[inkChoiceIndex];
            DialogueChoiceButton choiceButton = choiceButtons[choniceBuutonIndex];

            choiceButton.gameObject.SetActive(true);
            choiceButton.SetChoiceText(dialogueChoice.text.Trim());
            choiceButton.SetChoiceIndex(inkChoiceIndex);

            if(inkChoiceIndex == 0)
            {
                choiceButton.SelectButton();
                GameEventsManager.instance.dialogueEvents.UpdateChoiceIndex(0);
            }

            choniceBuutonIndex--;
        }
    }

    private void ResetPanel()
    {
        dialogueText.text = "";
    }
}