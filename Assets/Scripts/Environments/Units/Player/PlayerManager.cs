using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat))]

public class PlayerManager : MonoBehaviour
{
    #region Scene Singleton

    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        // 씬 싱글톤의 표준적인 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 컴포넌트 초기화
        status = new PlayerStatus();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }


    void OnDestroy()
    {
        // 자신이 Instance일 경우에만 null로 설정
        if (Instance == this)
            Instance = null;
    }
    #endregion

    private PlayerStatus status;
    public PlayerStatus Status
    {
        get { return status; }
    }

    private PlayerMovement movement;
    private PlayerCombat combat;

    //Animator animator;




    private void Start()
    {


        status.Reset();
        movement.status = status;
        combat.status = status;
    }

    void Update()
    {
    }

    public void Teleport(Vector3 destinationPosition)
    {
        StartCoroutine(movement.TeleportFadeEffect(destinationPosition));
    }

    public void StopPlayer()
    {
        //movement.UndoMoveCoroutine();
        StartCoroutine(status.StopAction());
        Debug.Log("stop player");
    }

    public void StartPlayer()
    {
        StartCoroutine(status.StartAction());
        Debug.Log("start player");
    }
}
