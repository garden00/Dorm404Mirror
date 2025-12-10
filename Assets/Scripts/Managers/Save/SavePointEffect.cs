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

        if (info.IsName("save_effect") && info.normalizedTime >= 1f)
        {
            isPlaying = false;
            if (sr != null)
                sr.enabled = false;

            anim.Play("Idle", 0, 0f);
        }
    }
}
