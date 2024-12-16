using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEditor;

[Serializable]
public class TriggerEvent : UnityEvent<GameObject> { }

public class TriggerDetector : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string[] tagsToDetect;
    [SerializeField] private bool debugMode;

    [Header("Trigger Events")]
    public TriggerEvent onTriggerEnter = new TriggerEvent();
    public TriggerEvent onTriggerStay = new TriggerEvent();
    public TriggerEvent onTriggerExit = new TriggerEvent();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldTrigger(other.tag))
        {
            if (debugMode) Debug.Log($"[TriggerDetector] Enter triggered by {other.gameObject.name} with tag {other.tag}");
            onTriggerEnter?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (ShouldTrigger(other.tag))
        {
            if (debugMode) Debug.Log($"[TriggerDetector] Stay triggered by {other.gameObject.name} with tag {other.tag}");
            onTriggerStay?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (ShouldTrigger(other.tag))
        {
            if (debugMode) Debug.Log($"[TriggerDetector] Exit triggered by {other.gameObject.name} with tag {other.tag}");
            onTriggerExit?.Invoke(other.gameObject);
        }
    }

    private bool ShouldTrigger(string tag)
    {
        // If no tags specified, trigger for all
        if (tagsToDetect == null || tagsToDetect.Length == 0) return true;

        // Check if the tag matches any in our list
        return Array.Exists(tagsToDetect, t => t == tag);
    }
}

// Optional: Custom editor to make tag selection easier
#if UNITY_EDITOR
[CustomEditor(typeof(TriggerDetector))]
public class TriggerDetectorEditor : Editor
{
    private SerializedProperty tagsToDetect;
    private SerializedProperty debugMode;
    private SerializedProperty onTriggerEnter;
    private SerializedProperty onTriggerStay;
    private SerializedProperty onTriggerExit;

    private void OnEnable()
    {
        tagsToDetect = serializedObject.FindProperty("tagsToDetect");
        debugMode = serializedObject.FindProperty("debugMode");
        onTriggerEnter = serializedObject.FindProperty("onTriggerEnter");
        onTriggerStay = serializedObject.FindProperty("onTriggerStay");
        onTriggerExit = serializedObject.FindProperty("onTriggerExit");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(debugMode);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tags to Detect", EditorStyles.boldLabel);

        // Show array size field
        EditorGUILayout.PropertyField(tagsToDetect, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onTriggerEnter);
        EditorGUILayout.PropertyField(onTriggerStay);
        EditorGUILayout.PropertyField(onTriggerExit);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif