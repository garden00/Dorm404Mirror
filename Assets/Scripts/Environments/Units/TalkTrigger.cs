using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkTrigger : MonoBehaviour
{
    [SerializeField]
    private int talkSceneId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null && collision.CompareTag("Player"))
        {
            Debug.Log("talk");
            TalkManager.Instance.StartDialogue(talkSceneId);
        }

    }
}
