using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private Transform exitPoint;

    private void Start()
    {
        exitPoint.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if(collision.CompareTag("Player"))
            {
                PlayerManager.Instance.Teleport(exitPoint.position);
            }
        }

    }
}
