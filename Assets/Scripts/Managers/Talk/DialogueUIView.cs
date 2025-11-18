using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 실제 대화창 UI 요소들을 제어
/// </summary>
public class DialogueUIView : MonoBehaviour, IDialogueView
{
    [SerializeField] private GameObject dialogueWindow;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject nextIndicator; // 다음 대사 알림

    // ITypeEffect가 사용할 수 있도록 TMP_Text 컴포넌트 노출
    public TMP_Text DialogueTextComponent => dialogueText;

    void Start()
    {
        // 시작 시 UI 숨김
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(false);
        }
    }

    public void Show()
    {
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(true);
        }
    }

    public void Hide()
    {
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(false);
        }
    }

    public void SetSpeakerName(string name)
    {
        speakerNameText.text = name;
    }

    public void SetPortrait(Sprite portrait)
    {
        if (portrait != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    public void UpdateText(string text)
    {
        dialogueText.text = text;
    }
}