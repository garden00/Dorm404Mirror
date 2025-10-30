using UnityEditor;
using UnityEngine;

// �� PropertyDrawer�� EightDirection Ÿ���� �׸� ������ �˸�
[CustomPropertyDrawer(typeof(EightDirection))]
public class EightDirectionDrawer : PropertyDrawer
{
    // 9�� ������ �̸��� �ν����Ϳ� ǥ���� �迭
    private static readonly string[] _directionNames =
    {
        "Right (0)",     // �ε��� 0
        "UpRight (1)",   // �ε��� 1
        "Up (2)",
        "UpLeft (3)",
        "Left (4)",
        "DownLeft (5)",
        "Down (6)",
        "DownRight (7)",
        "None (8)"       // �ε��� 8
    };

    // �ν����Ϳ��� GUI�� �ٽ� �׸��� �ٽ� �޼���
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. ������ ������Ƽ(_index)�� �̸����� ã���ϴ�.
        SerializedProperty indexProperty = property.FindPropertyRelative("_index");

        // 2. �ν����� ���̺��� ���� �׸��ϴ�.
        position = EditorGUI.PrefixLabel(position, label);

        // 3. ��Ӵٿ�(Popup)�� �׸��ϴ�.
        //    [���� ����] .byteValue -> .intValue
        //    EditorGUI.Popup�� int ���� �ް� int ���� ��ȯ�մϴ�.
        int newIndex = EditorGUI.Popup(position, indexProperty.intValue, _directionNames);

        // 4. ����ڰ� ��Ӵٿ�� �� ���� �����ߴٸ�,
        //    [���� ����] .byteValue -> .intValue
        if (newIndex != indexProperty.intValue)
        {
            // .intValue�� ���� �Ҵ��ϸ� Unity�� �ڵ����� byte �ʵ忡 �°� ��ȯ�� �ݴϴ�.
            indexProperty.intValue = newIndex;
        }
    }
}