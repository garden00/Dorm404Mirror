using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D2_AnimatorController : MonoBehaviour
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

    private Color defaultColor;
    private bool isDead = false;

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
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;

        float directionValue = 0f;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            directionValue = dir.x > 0 ? 3f : 1f;
        }
        else
        {
            directionValue = dir.y > 0 ? 2f : 0f;
        }

        anim.SetFloat("Direction", directionValue);
    }

    public void PlayIdle()
    {
        anim.Play("idle");
    }

    public void PlayHit()
    {
        if (isDead) return;
        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    public void PlayDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    public void PlayEffect(string effectName) { 
        // 나중에 연결
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
