using UnityEngine;

public class CreatureAnimatorController : MonoBehaviour
{
    private Animator anim;
    private void Awake() => anim = GetComponent<Animator>();

    public void PlayAttack() => anim.SetTrigger("Attack");
    public void PlayHit() => anim.SetTrigger("Hit");
    public void PlayDeath() => anim.SetTrigger("Death");
}
