using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider; // 인스펙터에서 Slider 연결

    /// <summary>
    /// 체력이 변할 때 외부(Enemy 스크립트)에서 이 함수를 호출
    /// </summary>
    /// <param name="currentHealth"></param>
    /// <param name="maxHealth"></param>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        hpSlider.value = (float)currentHealth / maxHealth;

        gameObject.SetActive(currentHealth > 0 && currentHealth < maxHealth);
    }
}
