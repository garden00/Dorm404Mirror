using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// 8���� + ����(None) ���¸� ��Ÿ���� 1����Ʈ ũ���� �淮 ����ü.
/// </summary>
[System.Serializable]
public struct EightDirection
{
    // --- ���� ������ ---

    // 0=Right, 1=UpRight, 2=Up, 3=UpLeft, 4=Left, 5=DownLeft, 6=Down, 7=DownRight, 8=None
    [SerializeField]
    private byte _index;

    // --- ���� �� (Vector ��ȯ��) ---

    // 1. (1, 1, 0) ���� (���� ����)
    private static readonly Vector3[] _gridVectorMap =
    {
        new Vector3(1, 0, 0),   // 0 (Right)
        new Vector3(1, 1, 0),   // 1 (UpRight)
        new Vector3(0, 1, 0),   // 2 (Up)
        new Vector3(-1, 1, 0),  // 3 (UpLeft)
        new Vector3(-1, 0, 0),  // 4 (Left)
        new Vector3(-1, -1, 0), // 5 (DownLeft)
        new Vector3(0, -1, 0),  // 6 (Down)
        new Vector3(1, -1, 0),  // 7 (DownRight)
        new Vector3(0, 0, 0)    // 8 (None)
    };

    // 2. ����ȭ��(Normalized) ���� (���� �̵�/������ ����)
    //    (�밢�� �̵��� 1.414�� �������� ���� ����)
    private static readonly Vector3[] _normalizedVectorMap;

    // --- 9���� ���� �ν��Ͻ� (��� ��: EightDirection.Right) ---
    public static readonly EightDirection Right = new EightDirection(0);
    public static readonly EightDirection UpRight = new EightDirection(1);
    public static readonly EightDirection Up = new EightDirection(2);
    public static readonly EightDirection UpLeft = new EightDirection(3);
    public static readonly EightDirection Left = new EightDirection(4);
    public static readonly EightDirection DownLeft = new EightDirection(5);
    public static readonly EightDirection Down = new EightDirection(6);
    public static readonly EightDirection DownRight = new EightDirection(7);
    public static readonly EightDirection None = new EightDirection(8);

    // --- ������ ---

    // ���� ������ (����ȭ �� �ʱ�ȭ)
    static EightDirection()
    {
        _normalizedVectorMap = new Vector3[_gridVectorMap.Length];
        for (int i = 0; i < _gridVectorMap.Length; i++)
        {
            _normalizedVectorMap[i] = _gridVectorMap[i].normalized;
        }
    }

    // private ������ (�ܺο��� new EightDirection(2) ���� ȣ�� ����)
    private EightDirection(byte index)
    {
        _index = index;
    }

    // --- ������Ƽ (���� ��ȯ) ---

    /// <summary>
    /// ����ȭ�� Vector3�� ��ȯ�մϴ�. (��: (0.707, 0.707, 0))
    /// (����, �̵� �ӵ� ��꿡 ����)
    /// </summary>
    public Vector3 VectorNormalized => _normalizedVectorMap[_index];

    /// <summary>
    /// (1, 1, 0) ���� ���� �׸��� Vector3�� ��ȯ�մϴ�.
    /// (Ÿ�ϸ� �̵� �� ���� ��꿡 ����)
    /// </summary>
    public Vector3 VectorGrid => _gridVectorMap[_index];
    public float x => VectorGrid.x;
    public float y => VectorGrid.y;

    // --- �ٽ� ���: ��ȯ (Vector3 <-> EightDirection) ---

    /// <summary>
    /// ������ Vector3�� ���� ����� 8�������� "����"�մϴ�.
    /// (��: ���̽�ƽ �Է� ��ȯ)
    /// </summary>
    /// <param name="vector">�Է� ���� (��: Input.GetAxis)</param>
    /// <param name="deadzone">�� �� �̸��� ���� ũ��� 'None'���� ó��</param>
    public static EightDirection FromVector3(Vector3 vector, float deadzone = 0.1f)
    {
        if (vector.sqrMagnitude < deadzone * deadzone)
        {
            return None;
        }

        // ������ ���� 45�� ������ �ݿø��Ͽ� �ε��� ���
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        int index = Mathf.RoundToInt(angle / 45f); // -8 ~ +8

        // C#�� % �����ڴ� ������ ���� �ٸ��� �۵��ϹǷ�, (+ 8)�� ����
        byte finalIndex = (byte)((index + 8) % 8);

        return new EightDirection(finalIndex);
    }

    public static EightDirection FromVector3(float _x, float _y, float _z, float deadzone = 0.1f)
    {
        if (_x*_x + _y*_y + _z*_z < deadzone * deadzone)
        {
            return None;
        }

        // ������ ���� 45�� ������ �ݿø��Ͽ� �ε��� ���
        float angle = Mathf.Atan2(_y, _x) * Mathf.Rad2Deg;
        int index = Mathf.RoundToInt(angle / 45f); // -8 ~ +8

        // C#�� % �����ڴ� ������ ���� �ٸ��� �۵��ϹǷ�, (+ 8)�� ����
        byte finalIndex = (byte)((index + 8) % 8);

        return new EightDirection(finalIndex);
    }

    // ---  ��û�Ͻ� ������ �����ε�  ---

    // 1. Vector3���� �Ͻ���(implicit) ��ȯ (���� ����)
    //    Vector3 myVec = EightDirection.Right; // �ڵ����� (1, 0, 0)�� ��
    public static implicit operator Vector3(EightDirection dir)
    {
        return _gridVectorMap[dir._index];
    }

    public static implicit operator Vector2(EightDirection dir)
    {
        return _gridVectorMap[dir._index];
    }

    // 2. Vector3���� �����(explicit) ��ȯ (������)
    //    EightDirection dir = (EightDirection)myJoystickInput;
    public static explicit operator EightDirection(Vector3 vec)
    {
        return FromVector3(vec);
    }

    // 3. �� ���� (==, !=)
    public static bool operator ==(EightDirection a, EightDirection b)
    {
        return a._index == b._index;
    }

    public static bool operator !=(EightDirection a, EightDirection b)
    {
        return a._index != b._index;
    }

    // 4. ���� ����/���� (���� ȸ��)
    //    EightDirection.Up + 1; // 1ĭ �ð���� ȸ�� -> UpRight
    //    EightDirection.Up - 2; // 2ĭ �ݽð���� ȸ�� -> Right
    public static EightDirection operator +(EightDirection dir, int steps)
    {
        if (dir._index == 8) return None; // None�� ȸ�� �Ұ�
        // (+ 8) �������� ���� steps�� ó��
        int newIndex = (dir._index + steps + 8) % 8;
        return new EightDirection((byte)newIndex);
    }

    public static EightDirection operator -(EightDirection dir, int steps)
    {
        return dir + (-steps);
    }

    // 5. ���� ���̳ʽ� (�ݴ� ����)
    //    -EightDirection.Right; // Left
    public static EightDirection operator -(EightDirection dir)
    {
        if (dir._index == 8) return None; // None�� �ݴ�� None
        int newIndex = (dir._index + 4) % 8; // �ݴ� ������ 4ĭ ����
        return new EightDirection((byte)newIndex);
    }

    //

    /// <summary>
    /// 45�� �ð�������� ȸ���մϴ�. (dir + 1 �� ����)
    /// </summary>
    public static EightDirection operator ++(EightDirection dir)
    {
        return dir + 1;
    }

    /// <summary>
    /// 45�� �ݽð�������� ȸ���մϴ�. (dir - 1 �� ����)
    /// </summary>
    public static EightDirection operator --(EightDirection dir)
    {
        return dir - 1;
    }

    // --- ��ƿ��Ƽ (Equals, GetHashCode ��) ---
    public override bool Equals(object obj)
    {
        return obj is EightDirection dir && _index == dir._index;
    }

    public override int GetHashCode()
    {
        return _index.GetHashCode();
    }

    public override string ToString()
    {
        // ����� �� "Up (2)" ������ ǥ��
        string[] names = { "Right", "UpRight", "Up", "UpLeft", "Left", "DownLeft", "Down", "DownRight", "None" };
        return $"{names[_index]} ({_index})";
    }
}