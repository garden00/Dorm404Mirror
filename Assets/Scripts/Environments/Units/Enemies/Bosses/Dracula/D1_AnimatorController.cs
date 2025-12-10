using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_AnimatorController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Transform player;

    [Header("Hit Settings")]
    [SerializeField] private float hitDuration = 0.5f;
    [SerializeField] private Color hitColor = new Color(0.3f, 0.5f, 1f, 1f);

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Effect")]
    [SerializeField] private DraculaEffectPlayer effectPlayer;

    private Color defaultColor;
    private bool isDead = false;
    private float lastDirection = 0f; // 직전 방향 저장

    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (body == null) body = GetComponent<SpriteRenderer>();
        if (player == null && PlayerManager.Instance != null)
            player = PlayerManager.Instance.transform;

        defaultColor = body.color;
    }

    void Update()
    {
        if (!isDead)
            UpdateDirection();
    }

    private void UpdateDirection()
    {
        // 1) player null이면 한 번 더 시도, 그래도 없으면 그냥 리턴
        if (player == null)
        {
            if (PlayerManager.Instance != null)
                player = PlayerManager.Instance.transform;

            if (player == null) return;
        }

        // 2) 2D 기준으로만 계산
        Vector2 diff = (Vector2)(player.position - transform.position);

        // 3) 거의 같은 위치면 방향 바꾸지 말고 직전 값 유지
        if (diff.sqrMagnitude < 0.0001f)
        {
            anim.SetFloat("Direction", lastDirection);
            return;
        }

        float directionValue;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            directionValue = diff.x > 0 ? 3f : 1f; // right : left
        }
        else
        {
            directionValue = diff.y > 0 ? 2f : 0f; // up : down
        }

        lastDirection = directionValue;
        anim.SetFloat("Direction", directionValue);
    }


    public void PlayIdle()
    {
        anim.Play("idle");
    }

    public void PlayHit()
    {
        var effect = GetComponentInChildren<DraculaEffectPlayer>();
        if (effect != null) effect.ForceHide();


        if (isDead) return;
        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    public void PlayDeath()
    {
        var effect = GetComponentInChildren<DraculaEffectPlayer>();
        if (effect != null) effect.ForceHide();

        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator HitRoutine()
    {
        body.color = hitColor;
        float t = 0f;

        while (t < hitDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        body.color = defaultColor;
    }

    private IEnumerator DeathRoutine()
    {
        float t = 0;
        Color start = body.color;

        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            body.color = new Color(start.r, start.g, start.b, a);

            t += Time.deltaTime;
            yield return null;
        }

        body.color = new Color(start.r, start.g, start.b, 0f);
        Destroy(gameObject);
    }
}
