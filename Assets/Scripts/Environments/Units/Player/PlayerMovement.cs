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
        // ���������� ���� �� ����
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastPressedAxis = Axis.Horizontal;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            lastPressedAxis = Axis.Vertical;
        }

        // �����̴� ���� �ƴ� ���� �� �Է��� �ޱ�
        if (!status.isAction)
        {
            float horizontal = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
            float vertical = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

            EightDirection inputDirection = EightDirection.None;

            //���������� ���� �� �켱���� �Է� ó��
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

            // ���� �Է� ���Ͱ� ���� ��쿡�� �������� ����
            if (inputDirection != EightDirection.None)
            {
                status.viewDirection = inputDirection;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, status.viewDirection, 1.0f, LayerMask.GetMask("Wall"));
                if (hit.collider != null)
                {
                    // �տ� �������� �ߴ�
                    return;
                }

                StartCoroutine(MoveToPosition(transform.position + status.viewDirection.VectorNormalized));
            }
        }
    }
    IEnumerator MoveToPosition(Vector3 _targetPosition)
    {
        status.isAction = true;
        while ((_targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = _targetPosition;
        status.isAction = false;
    }
}
