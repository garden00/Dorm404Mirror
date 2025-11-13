using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private GameObject shieldObject;

    [Header("Shield Sprites (8-way)")]
    [SerializeField] private Sprite shieldUp;
    [SerializeField] private Sprite shieldDown;
    [SerializeField] private Sprite shieldLeft;
    [SerializeField] private Sprite shieldRight;
    [SerializeField] private Sprite shieldUpLeft;
    [SerializeField] private Sprite shieldUpRight;
    [SerializeField] private Sprite shieldDownLeft;
    [SerializeField] private Sprite shieldDownRight;

    private SpriteRenderer shieldSR;
    private PlayerStatus status;
    private PlayerCombat combat;

    public Vector3 Direction { get; private set; }

    void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        shieldSR = shieldObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateDirection();
        UpdateSprite();
    }

    private void UpdateDirection()
    {
        float h = 0;
        float v = 0;

        if (Input.GetKey(KeyCode.A)) h = -1;
        if (Input.GetKey(KeyCode.D)) h = 1;
        if (Input.GetKey(KeyCode.S)) v = -1;
        if (Input.GetKey(KeyCode.W)) v = 1;

        Vector3 input = new Vector3(h, v, 0);

        // 입력 없으면 플레이어가 마지막으로 본 방향 유지
        if (input == Vector3.zero)
        {
            Direction = combat.status.viewDirection;
        }
        else
        {
            Direction = EightDirection.FromVector3(h, v, 0);
        }
    }

    private void UpdateSprite()
    {
        if (shieldSR == null) return;

        int sx = Direction.x > 0.1f ? 1 : (Direction.x < -0.1f ? -1 : 0);
        int sy = Direction.y > 0.1f ? 1 : (Direction.y < -0.1f ? -1 : 0);

        Sprite s = shieldDown;

        if (sx == 0 && sy > 0) s = shieldUp;
        else if (sx == 0 && sy < 0) s = shieldDown;
        else if (sx > 0 && sy == 0) s = shieldRight;
        else if (sx < 0 && sy == 0) s = shieldLeft;
        else if (sx > 0 && sy > 0) s = shieldUpRight;
        else if (sx < 0 && sy > 0) s = shieldUpLeft;
        else if (sx > 0 && sy < 0) s = shieldDownRight;
        else if (sx < 0 && sy < 0) s = shieldDownLeft;

        shieldSR.sprite = s;
        shieldObject.transform.localRotation = Quaternion.identity;
        shieldObject.transform.localPosition = Vector3.zero;
    }
}
