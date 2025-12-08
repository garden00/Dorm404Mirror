using System;
using UnityEngine;

public enum PlayerState
{ 
    Idle, 
    Moving, 
    Attacking, 
    Locked, 
    Dead 
}

[CreateAssetMenu(fileName = "PlayerStatusData", menuName = "Scriptables/PlayerStatusData")]
public class PlayerStatusData : ScriptableObject
{
    // --- Events ---
    public event Action<PlayerState> OnStateChanged;
    public event Action<Vector2> OnMoveDirectionChanged;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnChargingPowerChanged;
    public event Action OnPlayerDeath;
    public event Action<Vector3> OnHit;

    // --- State ---
    [field: SerializeField] public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    public bool IsMoveable => CurrentState == PlayerState.Idle;
    public bool IsActionable => CurrentState != PlayerState.Locked && CurrentState != PlayerState.Dead;
    public bool IsMoving => CurrentState == PlayerState.Moving;

    // --- Direction ---
    private EightDirection _viewDirection = EightDirection.Down;
    public EightDirection ViewDirection
    {
        get => _viewDirection;
        set
        {
            if (_viewDirection != value)
            {
                _viewDirection = value;
                OnMoveDirectionChanged?.Invoke(_viewDirection.VectorGrid);
            }
        }
    }

    // --- Stats ---
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxChargingPower = 30;

    public float speedMultiplier = 1f;

    // 런타임 변수 (Inspector에서 보려면 [SerializeField] 추가 가능)
    private int currentHealth;
    private int currentChargingPower;

    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke((float)currentHealth / maxHealth);

            if (currentHealth <= 0 && CurrentState != PlayerState.Dead)
            {
                SetState(PlayerState.Dead);
                OnPlayerDeath?.Invoke();
            }
        }
    }

    public int CurrentChargingPower
    {
        get => currentChargingPower;
        set
        {
            currentChargingPower = Mathf.Clamp(value, 0, maxChargingPower);
            OnChargingPowerChanged?.Invoke((float)currentChargingPower / maxChargingPower);
        }
    }
    public bool IsMaxCharged => currentChargingPower >= maxChargingPower;

    // --- Methods ---

    // 씬이 시작될 때 이 함수를 호출하여 구독을 모두 삭제
    public void ClearAllEvents()
    {
        OnStateChanged = null;
        OnMoveDirectionChanged = null;
        OnHealthChanged = null;
        OnChargingPowerChanged = null;
        OnPlayerDeath = null;
        OnHit = null;
    }

    // 게임이 처음 켜질 때 혹은 새 게임 시작 시 호출
    public void ResetData()
    {
        currentHealth = maxHealth;
        currentChargingPower = 0;
        ViewDirection = EightDirection.Down;
        CurrentState = PlayerState.Idle;
    }

    public void Healing(int amount = -1)
    {
        if (amount < 0)
            CurrentHealth = maxHealth;
        else
            CurrentHealth += amount;
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == PlayerState.Dead) return;
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void RaiseOnHitEvent(Vector3 hitSourcePos)
    {
        OnHit?.Invoke(hitSourcePos);
    }
}