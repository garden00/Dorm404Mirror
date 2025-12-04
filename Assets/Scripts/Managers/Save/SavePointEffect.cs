using UnityEngine;

public class SavePointEffect : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sr;

    private bool isPlaying = false;

    private void Awake()
    {
        if (sr != null)
            sr.enabled = false;
    }

    public void PlaySaveEffect()
    {
        if (sr != null)
            sr.enabled = true;

        if (anim != null)
            //anim.SetTrigger("Save");
            anim.Play("save_effect", 0, 0f);
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying) return;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        Debug.Log("현재 상태: " + info.IsName("save_effect")
                   + " / 해시: " + info.fullPathHash);

        if (info.IsName("save_effect") && info.normalizedTime >= 1f)
        {
            isPlaying = false;
            if (sr != null)
                sr.enabled = false;

            Debug.Log("*****sprite off");
            anim.Play("Idle", 0, 0f);
        }
    }
}
