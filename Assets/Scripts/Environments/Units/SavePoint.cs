using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            SavePointManager.Instance.OnStaySavePoint(this);
        }

    }
}
