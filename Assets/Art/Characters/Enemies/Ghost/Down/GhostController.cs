using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    Animator anim;
    float timer = 0f;
    float attackInterval = 4f; // 4초마다 공격

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackInterval)
        {
            anim.SetTrigger("Attack"); // Animator의 Attack 트리거 실행
            timer = 0f;
        }
    }
}
