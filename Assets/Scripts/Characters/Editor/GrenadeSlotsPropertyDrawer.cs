using UnityEngine;
using UnityEditor;

// [CustomPropertyDrawer(typeof(GrenadeSlots))]
public class GrenadeSlotsPropertyDrawer : PropertyDrawer
{
    private bool show = false;
    private float height = 0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect_foldout = new Rect(position.x, position.y, position.width, 20);
        show = EditorGUI.Foldout(rect_foldout, show, label, true, EditorStyles.foldoutHeader);
        height = 20f;

        if (show)
        {
            //////////////////////////////////////////////////////////////////
            /// collect Serialized Properties

            SerializedProperty sp_category = property.FindPropertyRelative("m_category");
            SerializedProperty sp_locked = property.FindPropertyRelative("m_fixedCategory");
            SerializedProperty sp_capacity = property.FindPropertyRelative("m_capacity");
            SerializedProperty sp_count = property.FindPropertyRelative("m_count");
            SerializedProperty sp_transforms = property.FindPropertyRelative("m_transforms");
            SerializedProperty sp_grenades = property.FindPropertyRelative("m_grenades");

            //////////////////////////////////////////////////////////////////
            /// calculate positions

            Rect rect_category = new Rect(position.x, position.y + height, position.width - 40, 20);
            Rect rect_locked = new Rect(rect_category.x + rect_category.width + 10, rect_category.y, 20, rect_category.height);
            height += 20;

            Rect rect_capacity = new Rect(position.x, position.y + height, position.width * .5f, 20);
            Rect rect_count = new Rect(rect_capacity.x + rect_capacity.width + 10, rect_capacity.y, rect_capacity.width, rect_capacity.height);
            height += 20;

            height += 10; // seperator
            Rect rect_transforms = new Rect(position.x, position.y + height, (position.width * .5f) - 10, 20 + (sp_capacity.intValue * 20)); // +20 for label
            Rect rect_grenades = new Rect(rect_transforms.x + rect_transforms.width + 10, rect_transforms.y, rect_transforms.width, rect_transforms.height);
            height += 10 + 20 + rect_transforms.height; // separator + label + height

            //////////////////////////////////////////////////////////////////
            /// display properties

            if (sp_locked.boolValue) EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(rect_category, sp_category, new GUIContent("Category"));
            if (sp_locked.boolValue) EditorGUI.EndDisabledGroup();

            EditorGUI.PropertyField(rect_locked, sp_locked, GUIContent.none);

            EditorGUI.PropertyField(rect_capacity, sp_capacity, new GUIContent("Capacity"));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(rect_count, sp_count, new GUIContent("Count"));
            EditorGUI.EndDisabledGroup();

            //////////////////////////////////////////////////////////////////
            /// display info

            int capacity = sp_capacity.intValue;

            Rect rect_transforms_label = new Rect(rect_transforms.x, rect_transforms.y, rect_transforms.width, 20);
            EditorGUI.LabelField(rect_transforms_label, "Transforms", EditorStyles.boldLabel);
            sp_transforms.arraySize = capacity;
            for (int i = 0; i < capacity; i++)
            {
                Rect rect_pos = new Rect(rect_transforms.x, rect_transforms.y + ((i + 1) * 20), rect_transforms.width, 20);
                EditorGUI.PropertyField(rect_pos, sp_transforms.GetArrayElementAtIndex(i), GUIContent.none);
            }

            Rect rect_grenades_label = new Rect(rect_grenades.x, rect_grenades.y, rect_grenades.width, 20);
            EditorGUI.LabelField(rect_grenades_label, "Grenades", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            sp_grenades.arraySize = capacity;
            for (int i = 0; i < capacity; i++)
            {
                Rect rect_pos = new Rect(rect_grenades.x, rect_grenades.y + ((i + 1) * 20), rect_grenades.width, 20);
                EditorGUI.PropertyField(rect_pos, sp_grenades.GetArrayElementAtIndex(i), GUIContent.none);
            }
            EditorGUI.EndDisabledGroup();
        }

        height += 2;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return height;
    }
}