using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            SceneController.Instance.LoadScene(targetSceneName);
        }
    }
}
