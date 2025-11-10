using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [NonSerialized]
    public PlayerStatus status;

    [SerializeField]
    private float moveSpeed = 10f;

    private enum Axis { Horizontal, Vertical }
    private Axis lastPressedAxis;

    Coroutine moveCoroutine;

    Vector3 prev_pos;

    private void Start()
    {
         PlayerManager.Instance.Status.OnHealthChanged += HandleDamageKnockback;
    }

    private void LateUpdate()
    {
        Move();
    }

    public IEnumerator TeleportFadeEffect(Vector3 newPos)
    {
        while (status.isAction)
        {
            yield return null;
        }
        status.isAction = true;

        UIManager.Instance.FadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);

        transform.position = newPos;
        status.isAction = false;

        UIManager.Instance.FadeIn(0.5f);
        yield return new WaitForSeconds(0.5f);
    }

    public void Move()
    {
        // 마지막으로 눌린 축 감지
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastPressedAxis = Axis.Horizontal;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            lastPressedAxis = Axis.Vertical;
        }

        // 움직이는 중이 아닐 때만 새 입력을 받기
        if (!status.isAction)
        {
            float horizontal = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            float vertical = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

            EightDirection inputDirection = EightDirection.None;

            //마지막으로 눌린 축 우선으로 입력 처리
            if (lastPressedAxis == Axis.Horizontal)
            {
                if (horizontal != 0)
                {
                    inputDirection = EightDirection.FromVector3(horizontal, 0, 0);
                }
                else if (vertical != 0)
                {
                    inputDirection = EightDirection.FromVector3(0, vertical, 0);
                    lastPressedAxis = Axis.Vertical;
                }
            }
            else // lastPressedAxis == Axis.Vertical
            {
                if (vertical != 0)
                {
                    inputDirection = EightDirection.FromVector3(0, vertical, 0);
                }
                else if (horizontal != 0)
                {
                    inputDirection = EightDirection.FromVector3(horizontal, 0, 0);
                    lastPressedAxis = Axis.Horizontal;
                }
            }

            // 최종 입력 벡터가 있을 경우에만 움직임을 시작
            if (inputDirection != EightDirection.None)
            {
                status.viewDirection = inputDirection;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, status.viewDirection, 1.0f, LayerMask.GetMask("Wall"));
                if (hit.collider != null)
                {
                    // 앞에 막혔으면 중단
                    return;
                }

                moveCoroutine = StartCoroutine(MoveToPosition(transform.position + status.viewDirection.VectorNormalized));
            }
        }
    }
    IEnumerator MoveToPosition(Vector3 _targetPosition)
    {
        status.isAction = true;

        bool isInt = Mathf.Approximately(transform.position.x % 1, 0f) &&
             Mathf.Approximately(transform.position.y % 1, 0f) &&
             Mathf.Approximately(transform.position.z % 1, 0f);

        if (isInt)
            prev_pos = transform.position;

        while ((_targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = _targetPosition;
        status.isAction = false;
        moveCoroutine = null;
    }

    // 이벤트 처리 함수
    private void HandleDamageKnockback(float hp)
    {
        if (hp == 1) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            StartCoroutine(MoveToPosition(prev_pos));
            return;
        }


        Vector3 target_pos = transform.position - status.viewDirection;
        StartCoroutine(MoveToPosition(target_pos));

    }
}
