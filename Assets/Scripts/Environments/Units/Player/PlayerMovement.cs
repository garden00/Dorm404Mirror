
using System.Collections;
using UnityEngine;

// 할것 조작감 개선

public class PlayerMovement : MonoBehaviour
{
    private PlayerStatusData status;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private LayerMask wallLayer;

    private Coroutine moveCoroutine;
    private Vector3 preMovePosition;
    private enum Axis { None, Horizontal, Vertical }
    private Axis lastPressedAxis = Axis.None;

    public void Initialize(PlayerStatusData playerStatus)
    {
        status = playerStatus;

        status.OnHit += HandleKnockback;
    }

    private void OnDestroy()
    {
        if (status != null)
            status.OnHit -= HandleKnockback;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        int h = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        int v = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        bool newH = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
        bool newV = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);

        if (newH) lastPressedAxis = Axis.Horizontal;
        if (newV) lastPressedAxis = Axis.Vertical;

        if (lastPressedAxis == Axis.Horizontal && h == 0 && v != 0)
        {
            lastPressedAxis = Axis.Vertical;
        }
        else if (lastPressedAxis == Axis.Vertical && v == 0 && h != 0)
        {
            lastPressedAxis = Axis.Horizontal;
        }

        EightDirection inputDir = EightDirection.None;

        if (lastPressedAxis == Axis.Horizontal && h != 0)
        {
            inputDir = EightDirection.FromVector3(h, 0, 0);
        }
        else if (lastPressedAxis == Axis.Vertical && v != 0)
        {
            inputDir = EightDirection.FromVector3(0, v, 0);
        }

        if (!status.IsMoveable) return;

        if (inputDir != EightDirection.None)
        {
            TryMove(inputDir);
        }
    }

    private void TryMove(EightDirection dir)
    {
        status.ViewDirection = dir;

        Vector3 targetPos = transform.position + dir;

        // 벽 충돌 체크
        if (Physics2D.Raycast(transform.position, dir, 1.0f, wallLayer)) return;

        if (status.IsMoveable)
        {
            moveCoroutine = StartCoroutine(MoveRoutine(targetPos));
        }
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition)
    {
        status.SetState(PlayerState.Moving);
        preMovePosition = transform.position; // 롤백용 위치 저장

        // 정확한 그리드 이동을 위해 sqrMagnitude 사용
        while ((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * status.speedMultiplier * Time.deltaTime);

            yield return null;
        }

        transform.position = targetPosition;


        if (status.IsMoving)
        {
            status.SetState(PlayerState.Idle);
        }
        else
        {
            // 이동 도중 상태가 변했을 경우
            // 그 변한 상태 유지
        }

        moveCoroutine = null;
    }

    private void HandleKnockback(Vector3 hitSourcePos)
    {
        // 이동 중 피격 시 이동 취소 및 롤백
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;

            // 원래 위치로 튕겨나가는 연출
            StartCoroutine(KnockbackRoutine(preMovePosition));
        }
        else
        {
            // 제자리에 서있다가 맞았을 때 뒤로 밀림
            hitSourcePos = EightDirection.FromVector3(hitSourcePos);
            if (Physics2D.Raycast(transform.position, hitSourcePos, 1.0f, wallLayer)) return;
            moveCoroutine = StartCoroutine(KnockbackRoutine(transform.position + hitSourcePos));
        }
    }

    private IEnumerator KnockbackRoutine(Vector3 rollbackPos)
    {
        status.SetState(PlayerState.Moving);
        // 빠르게 원래 위치로 복귀
        float knockbackSpeed = moveSpeed * 2f;
        while ((rollbackPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, rollbackPos, knockbackSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = rollbackPos;

        if (status.IsMoving)
        {
            status.SetState(PlayerState.Idle);
        }
        else
        {
            // 이동 도중 상태가 변했을 경우
            // 그 변한 상태 유지
        }

        moveCoroutine = null;
    }

    public IEnumerator TeleportSequence(Vector3 newPos)
    {
        status.SetState(PlayerState.Locked);

        UIManager.Instance?.FadeOut(0.3f);
        yield return new WaitForSeconds(0.8f);

        transform.position = newPos;
        CameraManager.Instance?.Teleport(newPos);

        UIManager.Instance?.FadeIn(0.5f);
        yield return new WaitForSeconds(0.5f);

        status.SetState(PlayerState.Idle);
    }
}