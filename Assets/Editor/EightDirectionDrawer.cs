using UnityEditor;
using UnityEngine;

// 이 PropertyDrawer가 EightDirection 타입을 그릴 것임을 알림
[CustomPropertyDrawer(typeof(EightDirection))]
public class EightDirectionDrawer : PropertyDrawer
{
    // 9개 방향의 이름을 인스펙터에 표시할 배열
    private static readonly string[] _directionNames =
    {
        "Right (0)",     // 인덱스 0
        "UpRight (1)",   // 인덱스 1
        "Up (2)",
        "UpLeft (3)",
        "Left (4)",
        "DownLeft (5)",
        "Down (6)",
        "DownRight (7)",
        "None (8)"       // 인덱스 8
    };

    // 인스펙터에서 GUI를 다시 그리는 핵심 메서드
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. 수정할 프로퍼티(_index)를 이름으로 찾습니다.
        SerializedProperty indexProperty = property.FindPropertyRelative("_index");

        // 2. 인스펙터 레이블을 먼저 그립니다.
        position = EditorGUI.PrefixLabel(position, label);

        // 3. 드롭다운(Popup)을 그립니다.
        //    [오류 수정] .byteValue -> .intValue
        //    EditorGUI.Popup은 int 값을 받고 int 값을 반환합니다.
        int newIndex = EditorGUI.Popup(position, indexProperty.intValue, _directionNames);

        // 4. 사용자가 드롭다운에서 새 값을 선택했다면,
        //    [오류 수정] .byteValue -> .intValue
        if (newIndex != indexProperty.intValue)
        {
            // .intValue에 값을 할당하면 Unity가 자동으로 byte 필드에 맞게 변환해 줍니다.
            indexProperty.intValue = newIndex;
        }
    }
}