using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Fish))]
public class FishEditor : Editor
{
    private Editor fishDataEditor = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SerializedProperty serializedData = serializedObject.FindProperty("data");

        Object data = serializedData.objectReferenceValue;

        Editor.CreateCachedEditor(data, null, ref fishDataEditor);

        fishDataEditor.OnInspectorGUI();
    }
}
