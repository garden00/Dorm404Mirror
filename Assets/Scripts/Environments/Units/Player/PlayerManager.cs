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
        // �� �̱����� ǥ������ ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ������Ʈ �ʱ�ȭ
        status = new PlayerStatus();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }


    void OnDestroy()
    {
        // �ڽ��� Instance�� ��쿡�� null�� ����
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

}
