using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    [SerializeField] int endingTalkId;
    void Start()
    {
        TalkManager.Instance.StartDialogue(endingTalkId);
        TalkManager.Instance.OnTalkEnded += ending;
    }

    private void OnDestroy()
    {
        TalkManager.Instance.OnTalkEnded -= ending;
    }

    void ending()
    {
        SceneController.Instance.LoadScene("Title");
    }
}
