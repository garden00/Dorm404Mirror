using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IDamageable
{
    [SerializeField]
    private bool breakable;

    public void ReceiveAttack(DamageInfo damageInfo)
    {
        if(breakable)
        {
            //break
        }
        return;
    }
}
