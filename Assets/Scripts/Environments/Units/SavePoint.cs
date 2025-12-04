using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || !collision.CompareTag("Player"))
            return;

        if (SavePointManager.Instance == null)
            return;

        SavePointManager.Instance.OnEnterSavePoint(this);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null || !collision.CompareTag("Player"))
            return;

        if (SavePointManager.Instance == null)
            return;

        SavePointManager.Instance.OnExitSavePoint(this);
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (collision != null && collision.CompareTag("Player"))
    //    {
    //        SavePointManager.Instance.OnStaySavePoint(this);
    //    }
    //}
}
