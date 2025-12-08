using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(PlayerAnimationController))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Data Asset")]
    [SerializeField] private PlayerStatusData statusData;
    public PlayerStatusData Status => statusData;

    [Header("Debug")]
    [SerializeField] private bool resetDataOnStart = false; // 테스트용: 체크하면 시작할 때 피 채움

    // Components
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerAnimationController AnimationController { get; private set; }
    public ShieldController ShieldController { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 컴포넌트 가져오기
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        AnimationController = GetComponent<PlayerAnimationController>();
        ShieldController = GetComponent<ShieldController>();

        // 데이터 초기화 (옵션)
        if (resetDataOnStart)
        {
            statusData.ResetData();
        }

        statusData.ClearAllEvents();

        // 컴포넌트 초기화
        Movement.Initialize(statusData);
        Combat.Initialize(statusData);
        AnimationController.Initialize(statusData);
        ShieldController.Initialize(statusData);
    }

    private void Start()
    {
        TalkManager.Instance.OnTalkStarted += SetLocked;
        TalkManager.Instance.OnTalkEnded += SetIdle;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (TalkManager.Instance != null)
        {
            TalkManager.Instance.OnTalkStarted -= SetLocked;
            TalkManager.Instance.OnTalkEnded -= SetIdle;
        }
    }

    // --- Public Methods ---
    public void Teleport(Vector3 destinationPosition)
    {
        StartCoroutine(Movement.TeleportSequence(destinationPosition));
    }

    public void SetIdle()
    {
        statusData.SetState(PlayerState.Idle);
    }

    public void SetLocked()
    {
        statusData.SetState(PlayerState.Locked);
    }

    public void Charge(float time)
    {
        Combat.chage(time);
    }
}