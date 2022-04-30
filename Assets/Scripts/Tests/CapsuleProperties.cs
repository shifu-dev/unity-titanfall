using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CapsuleProperties))]
public class CapsulePropertiesEditor : Editor
{
    public void OnEnable()
    {
        CapsuleProperties target = (CapsuleProperties)this.target;
        target.Awake();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CapsuleProperties target = (CapsuleProperties)this.target;
        EditorGUI.BeginDisabledGroup(true);

        EditorGUILayout.Separator();
        EditorGUILayout.Vector3Field("LocalScale", target.transform.localScale);
        EditorGUILayout.Vector3Field("WorldScale", target.transform.lossyScale);

        EditorGUILayout.Separator();
        EditorGUILayout.TextField("PriorityUnscaled", target.IsHeightPrimaryUnscaled ? "Height" : "Radius");
        EditorGUILayout.FloatField("CylinderHeightUnscaled", target.GetCylinderHeightUnscaled);
        EditorGUILayout.FloatField("RadiusUnscaled", target.GetRadiusUnscaled);

        EditorGUILayout.Separator();
        EditorGUILayout.TextField("Priority", target.IsHeightPrimary ? "Height" : "Radius");
        EditorGUILayout.FloatField("Height", target.GetHeight);
        EditorGUILayout.FloatField("Radius", target.GetRadius);

        EditorGUI.EndDisabledGroup();
    }
}

[RequireComponent(typeof(CapsuleCollider))]
public class CapsuleProperties : MonoBehaviour
{
    private CapsuleCollider Capsule;

    public void Awake()
    {
        Capsule = gameObject.GetComponent<CapsuleCollider>();
    }

    public bool IsHeightPrimaryUnscaled { get => Capsule.height >= Capsule.radius * 2; }
    public float GetRadiusUnscaled { get => Capsule.radius; }
    public float GetCylinderHeightUnscaled { get => Capsule.height; }

    public bool IsHeightPrimary
    {
        get
        {
            return GetHeight >= GetRadius * 2;
        }
    }

    public float GetRadius
    {
        get
        {
            Vector3 worldScale = Capsule.transform.lossyScale;
            return Capsule.radius * Mathf.Max(worldScale.x, worldScale.z);
        }
    }

    public float GetHeight
    {
        get
        {
            return Capsule.height * Capsule.transform.lossyScale.y;
        }
    }
}