using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Image healthBar;

    void Start()
    {
        PlayerManager.Instance.Status.OnHealthChanged += UpdateHealth;
    }

    void OnDisable()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.Status.OnHealthChanged -= UpdateHealth;
        }
    }

    void UpdateHealth(float health)
    {
        healthBar.fillAmount = health;
    }
}
