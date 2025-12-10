using System.Collections;
using UnityEngine;

public class DraculaEffectPlayer : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sprite;

    [Header("Durations")]
    [SerializeField] private float castDuration = 0.4f;
    [SerializeField] private float teleportDuration = 0.5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float berserkFlashDuration = 0.4f;

    private Coroutine playRoutine;

    void Awake()
    {
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
    }


    public void PlayCast()
    {
        PlayState("Cast", castDuration, Color.white);
    }

    public void PlayTeleport()
    {
        PlayState("Teleport", teleportDuration, Color.white);
    }

    public void PlayDash()
    {
        PlayState("Dash", dashDuration, Color.black);
    }

    public void PlayBerserkFlash()
    {
        PlayState("BerserkFlash", berserkFlashDuration, sprite.color);
    }

    public void ForceHide()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (sprite != null)
            sprite.enabled = false;
    }

    private void PlayState(string stateName, float duration, Color color)
    {
        if (sprite == null || anim == null) return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        sprite.color = color;
        sprite.enabled = true;

        // 상태 이름으로 바로 재생 (0번째 레이어, 첫 프레임부터)
        anim.Play(stateName, 0, 0f);

        playRoutine = StartCoroutine(AutoHideAfter(duration));
    }

    private IEnumerator AutoHideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (sprite != null)
            sprite.enabled = false;

        playRoutine = null;
    }
}
