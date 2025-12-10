using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingTrigger : MonoBehaviour
{
    [SerializeField] Image sf;
    [SerializeField] int endingTalkId;
    void Start()
    {
        TalkManager.Instance.StartDialogue(endingTalkId);
        TalkManager.Instance.ending = sf;
        TalkManager.Instance.OnTalkEnded += ending;
    }

    private void OnDestroy()
    {
        TalkManager.Instance.OnTalkEnded -= ending;
        TalkManager.Instance.ending = null;
    }

    void ending()
    {
        SceneController.Instance.LoadScene("Title");
    }
}
