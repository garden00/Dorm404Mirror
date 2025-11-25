using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkTrigger : MonoBehaviour
{
    [SerializeField]
    private int talkSceneId;

    bool isTalked = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null && collision.CompareTag("Player") && !isTalked)
        {
            isTalked = true;
            TalkManager.Instance.StartDialogue(talkSceneId);
        }

    }
}
