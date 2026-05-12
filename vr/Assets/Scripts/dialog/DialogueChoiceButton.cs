using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueChoiceButton : MonoBehaviour, ISelectHandler
{
    [Header("Components")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI choiceText;

    private int choiceIndex = -1;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    public void SetChoiceText(string choiceTextString)
    {
        choiceText.text = choiceTextString;
    }
    public void SetChoiceIndex(int choiceIndex)
    {
        this.choiceIndex = choiceIndex;
    }

    public void SelectButton()
    {
        button.Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        GameEventsManager.instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameEventsManager.instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked();
    }

    private void OnClicked()
    {
        GameEventsManager.instance.dialogueEvents.UpdateChoiceIndex(choiceIndex);
        GameEventsManager.instance.inputEvents.InteractPressed();
    }
}
