using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    [SerializeField] private GameObject shopUI;

    private ShopPanelUI shopPanelUI;

    private Story story;

    private int currentChoiceIndex = -1;

    private bool dialoguePlaying = false;

    private void Start()
    {
        story = new Story(inkJson.text);

        if (!shopUI)
        {
            Debug.Log("DialogueManager: shopUI not assigned");
        }
        else
        {
            shopPanelUI = shopUI.GetComponent<ShopPanelUI>();
            if(shopPanelUI == null)
            {
                Debug.LogWarning("DialogueManager: ShopPanelUI component missing on shopUI");
            }
        }
            
        story.BindExternalFunction("OpenShop", () => {
            Debug.Log("openShop");
            if (shopPanelUI != null)
                shopPanelUI.OpenShopPressed();
        });
    }

    private void OnEnable()
    {
        var gem = GameEventsManager.instance;
        if (gem.dialogueEvents != null)
        {
            GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
            GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
        }
        else
        {
            Debug.LogWarning("GameEventsManager.instance.dialogueEvents is null");
        }
        if (gem.inputEvents != null)
        {
            GameEventsManager.instance.inputEvents.onInteractPressed += InteractPressed;
        }
        else
        {
            Debug.LogWarning("GameEventsManager.instance.onInteractPressed is null");
        }
    }

    private void OnDisable()
    {
        var gem = GameEventsManager.instance;
        if (gem.dialogueEvents != null)
        {
            GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
            GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;
        }
        if (gem.inputEvents != null)
        {
            GameEventsManager.instance.inputEvents.onInteractPressed -= InteractPressed;
        }
    }

    private void UpdateChoiceIndex(int choiceIndex)
    {
        this.currentChoiceIndex = choiceIndex;
    }

    private void InteractPressed(InputEventContext inputEventContext)
    {
        if (!inputEventContext.Equals(InputEventContext.DIALOGUE))
        {
            return;
        }

        ContinueOrExitStory();
    }

    public void TalkButtonPressed(string knotName)
    {
        EnterDialogue(knotName);
    }

    private void EnterDialogue(string knotName)
    {
        if (dialoguePlaying)
        {
            Debug.LogWarning("DialogueManager: dialogue playing!");
            return;
        }

        dialoguePlaying = true;

        GameEventsManager.instance.dialogueEvents.DialogueStarted();

        GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DIALOGUE);

        if (!string.IsNullOrEmpty(knotName))
        {
            Debug.Log($"DialogueManager: ChoosePathString {knotName}");
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("Knot name was the empty string when entering dialogue.");
        }

        Debug.Log($"DialogueManager: ContinueOrExitStory");
        ContinueOrExitStory();
    }

    private void ContinueOrExitStory()
    {
        if (story.currentChoices.Count > 0 && currentChoiceIndex != -1)
        {
            story.ChooseChoiceIndex(currentChoiceIndex);

            currentChoiceIndex = -1;
        }

        if (story.canContinue)
        {
            string dialogueLine = story.Continue();

            while (IsLineBlank(dialogueLine) && story.canContinue)
            {
                dialogueLine = story.Continue();
            }

            if (IsLineBlank(dialogueLine) && !story.canContinue)
            {
                ExitDialogue();
            }
            else
            {
                GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices);
            }
        }
        else if (story.currentChoices.Count == 0)
        {
            ExitDialogue();
        }
    }

    private void ExitDialogue()
    {
        Debug.Log("Exiting Dialoggue");

        dialoguePlaying = false;

        GameEventsManager.instance.dialogueEvents.DialogueFinished();

        GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DEFAULT);

        story.ResetState();
    }

    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
    }
}
