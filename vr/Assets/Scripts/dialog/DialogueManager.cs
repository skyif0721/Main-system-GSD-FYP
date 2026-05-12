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

    private Story story;

    private int currentChoiceIndex = -1;

    private bool dialoguePlaying = false;

    private void Start()
    {
        story = new Story(inkJson.text);

        if (!shopUI)
        {
            Debug.Log("Dialog no shopUI");
            return;
        }
        ;
        shopUI.GetComponent<ShopPanelUI>();

        story.BindExternalFunction("OpenShop", () => {
            Debug.Log("openShop");
            shopUI.GetComponent<ShopPanelUI>().OpenShopPressed();
        });
    }

    private void OnEnable()
    {
        if(GameEventsManager.instance != null)
        {
            GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
            GameEventsManager.instance.inputEvents.onInteractPressed += InteractPressed;
            GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
        }
        else
        {
            Debug.LogWarning("GameEventsManager.instance is null");
        }
        
    }

    private void OnDisable()
    {
        if(GameEventsManager.instance != null) {
            GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
            GameEventsManager.instance.inputEvents.onInteractPressed -= InteractPressed;
            GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;
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
            return;
        }

        dialoguePlaying = true;

        GameEventsManager.instance.dialogueEvents.DialogueStarted();

        GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DIALOGUE);

        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("Knot name was the empty string when entering dialogue.");
        }

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
