using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// 8방향 + 정지(None) 상태를 나타내는 1바이트 크기의 경량 구조체.
/// </summary>
[System.Serializable]
public struct EightDirection
{
    // --- 내부 데이터 ---

    // 0=Right, 1=UpRight, 2=Up, 3=UpLeft, 4=Left, 5=DownLeft, 6=Down, 7=DownRight, 8=None
    [SerializeField]
    private byte _index;

    // --- 정적 맵 (Vector 변환용) ---

    // 1. (1, 1, 0) 벡터 (원본 벡터)
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

    // 2. 정규화된(Normalized) 벡터 (실제 이동/물리에 권장)
    //    (대각선 이동이 1.414배 빨라지는 것을 방지)
    private static readonly Vector3[] _normalizedVectorMap;

    // --- 9가지 정적 인스턴스 (사용 예: EightDirection.Right) ---
    public static readonly EightDirection Right = new EightDirection(0);
    public static readonly EightDirection UpRight = new EightDirection(1);
    public static readonly EightDirection Up = new EightDirection(2);
    public static readonly EightDirection UpLeft = new EightDirection(3);
    public static readonly EightDirection Left = new EightDirection(4);
    public static readonly EightDirection DownLeft = new EightDirection(5);
    public static readonly EightDirection Down = new EightDirection(6);
    public static readonly EightDirection DownRight = new EightDirection(7);
    public static readonly EightDirection None = new EightDirection(8);

    // --- 생성자 ---

    // 정적 생성자 (정규화 맵 초기화)
    static EightDirection()
    {
        _normalizedVectorMap = new Vector3[_gridVectorMap.Length];
        for (int i = 0; i < _gridVectorMap.Length; i++)
        {
            _normalizedVectorMap[i] = _gridVectorMap[i].normalized;
        }
    }

    // private 생성자 (외부에서 new EightDirection(2) 같은 호출 방지)
    private EightDirection(byte index)
    {
        _index = index;
    }

    // --- 프로퍼티 (벡터 변환) ---

    /// <summary>
    /// 정규화된 Vector3를 반환합니다. (예: (0.707, 0.707, 0))
    /// (물리, 이동 속도 계산에 권장)
    /// </summary>
    public Vector3 VectorNormalized => _normalizedVectorMap[_index];

    /// <summary>
    /// (1, 1, 0) 같은 원본 그리드 Vector3를 반환합니다.
    /// (타일맵 이동 등 정수 계산에 유용)
    /// </summary>
    public Vector3 VectorGrid => _gridVectorMap[_index];
    public float x => VectorGrid.x;
    public float y => VectorGrid.y;

    // --- 핵심 기능: 변환 (Vector3 <-> EightDirection) ---

    /// <summary>
    /// 임의의 Vector3를 가장 가까운 8방향으로 "스냅"합니다.
    /// (예: 조이스틱 입력 변환)
    /// </summary>
    /// <param name="vector">입력 벡터 (예: Input.GetAxis)</param>
    /// <param name="deadzone">이 값 미만의 벡터 크기는 'None'으로 처리</param>
    public static EightDirection FromVector3(Vector3 vector, float deadzone = 0.1f)
    {
        if (vector.sqrMagnitude < deadzone * deadzone)
        {
            return None;
        }

        // 각도를 구해 45도 단위로 반올림하여 인덱스 계산
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        int index = Mathf.RoundToInt(angle / 45f); // -8 ~ +8

        // C#의 % 연산자는 음수에 대해 다르게 작동하므로, (+ 8)로 보정
        byte finalIndex = (byte)((index + 8) % 8);

        return new EightDirection(finalIndex);
    }

    public static EightDirection FromVector3(float _x, float _y, float _z, float deadzone = 0.1f)
    {
        if (_x*_x + _y*_y + _z*_z < deadzone * deadzone)
        {
            return None;
        }

        // 각도를 구해 45도 단위로 반올림하여 인덱스 계산
        float angle = Mathf.Atan2(_y, _x) * Mathf.Rad2Deg;
        int index = Mathf.RoundToInt(angle / 45f); // -8 ~ +8

        // C#의 % 연산자는 음수에 대해 다르게 작동하므로, (+ 8)로 보정
        byte finalIndex = (byte)((index + 8) % 8);

        return new EightDirection(finalIndex);
    }

    // ---  요청하신 연산자 오버로딩  ---

    // 1. Vector3로의 암시적(implicit) 변환 (가장 편리함)
    //    Vector3 myVec = EightDirection.Right; // 자동으로 (1, 0, 0)이 됨
    public static implicit operator Vector3(EightDirection dir)
    {
        return _gridVectorMap[dir._index];
    }

    public static implicit operator Vector2(EightDirection dir)
    {
        return _gridVectorMap[dir._index];
    }

    // 2. Vector3에서 명시적(explicit) 변환 (스냅핑)
    //    EightDirection dir = (EightDirection)myJoystickInput;
    public static explicit operator EightDirection(Vector3 vec)
    {
        return FromVector3(vec);
    }

    // 3. 비교 연산 (==, !=)
    public static bool operator ==(EightDirection a, EightDirection b)
    {
        return a._index == b._index;
    }

    public static bool operator !=(EightDirection a, EightDirection b)
    {
        return a._index != b._index;
    }

    // 4. 정수 덧셈/뺄셈 (방향 회전)
    //    EightDirection.Up + 1; // 1칸 시계방향 회전 -> UpRight
    //    EightDirection.Up - 2; // 2칸 반시계방향 회전 -> Right
    public static EightDirection operator +(EightDirection dir, int steps)
    {
        if (dir._index == 8) return None; // None은 회전 불가
        // (+ 8) 보정으로 음수 steps도 처리
        int newIndex = (dir._index + steps + 8) % 8;
        return new EightDirection((byte)newIndex);
    }

    public static EightDirection operator -(EightDirection dir, int steps)
    {
        return dir + (-steps);
    }

    // 5. 단항 마이너스 (반대 방향)
    //    -EightDirection.Right; // Left
    public static EightDirection operator -(EightDirection dir)
    {
        if (dir._index == 8) return None; // None의 반대는 None
        int newIndex = (dir._index + 4) % 8; // 반대 방향은 4칸 차이
        return new EightDirection((byte)newIndex);
    }

    //

    /// <summary>
    /// 45도 시계방향으로 회전합니다. (dir + 1 과 동일)
    /// </summary>
    public static EightDirection operator ++(EightDirection dir)
    {
        return dir + 1;
    }

    /// <summary>
    /// 45도 반시계방향으로 회전합니다. (dir - 1 과 동일)
    /// </summary>
    public static EightDirection operator --(EightDirection dir)
    {
        return dir - 1;
    }

    // --- 유틸리티 (Equals, GetHashCode 등) ---
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
        // 디버깅 시 "Up (2)" 등으로 표시
        string[] names = { "Right", "UpRight", "Up", "UpLeft", "Left", "DownLeft", "Down", "DownRight", "None" };
        return $"{names[_index]} ({_index})";
    }
}