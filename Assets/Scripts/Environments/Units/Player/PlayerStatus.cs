using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus
{
    public void Reset()
    {
        CurrentHealth = maxHealth;
        isAction = false;
        viewDirection = EightDirection.Down;
    }

    public void Healing()
    {
        CurrentHealth = maxHealth;
    }

    public event Action OnPlayerDeath;
    public event Action<float> OnHealthChanged;

    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (currentHealth < 0)
            {
                currentHealth = 0;

                OnPlayerDeath?.Invoke();
            }
            else if (currentHealth > maxHealth) currentHealth = maxHealth;

            //Debug.Log("player hp : " + currentHealth);

            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        }
    }

    public event Action<float> OnChargingPowerChanged;

    [SerializeField]
    private int maxChargingPower = 30;
    private int currentChargingPower;
    public int CurrentChargingPower
    {
        get => currentChargingPower;
        set
        {
            currentChargingPower = value;

            if (currentChargingPower < 0) currentChargingPower = 0;
            else if (currentChargingPower > maxChargingPower) currentChargingPower = maxChargingPower;

            Debug.Log("charing power : " + currentChargingPower);

            OnChargingPowerChanged?.Invoke((float)currentChargingPower / maxChargingPower);
        }
    }
    public bool ChargedMax
    {
        get => currentChargingPower == maxChargingPower;
    }


    [NonSerialized]
    public bool isAction;

    [NonSerialized]
    public EightDirection viewDirection;
}
